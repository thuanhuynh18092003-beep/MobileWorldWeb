using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Route("thanh-toan")]
public class CheckoutController(ShopDbContext db, CartService cart) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var current = await cart.GetAsync();
        if (current.Count == 0) return RedirectToAction("Index", "Cart");
        return View(new CheckoutViewModel { Cart = current, CustomerName = User.Identity?.IsAuthenticated == true ? User.Identity.Name ?? "" : "", Email = User.FindFirstValue(ClaimTypes.Email) ?? "" });
    }

    [HttpPost(""), ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model, string? command)
    {
        model.Cart = await cart.GetAsync();
        if (model.Cart.Count == 0) return RedirectToAction("Index", "Cart");
        model.DiscountAmount = 0;
        model.DiscountCode = model.DiscountCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrEmpty(model.DiscountCode))
        {
            var offerPreview = await db.DiscountOffers.AsNoTracking().FirstOrDefaultAsync(x => x.Code == model.DiscountCode);
            var offerError = offerPreview is null ? "Mã ưu đãi không tồn tại." : DiscountCalculator.Validate(offerPreview, model.Cart.Total, DateTime.UtcNow);
            if (offerError is not null) ModelState.AddModelError(nameof(model.DiscountCode), offerError);
            else model.DiscountAmount = DiscountCalculator.Calculate(offerPreview!, model.Cart.Total);
        }
        if (command == "preview")
        {
            if (string.IsNullOrWhiteSpace(model.DiscountCode)) ModelState.AddModelError(nameof(model.DiscountCode), "Vui lòng nhập mã ưu đãi.");
            return View(model);
        }
        if (model.PaymentMethod != "COD" && model.PaymentMethod != "BankTransfer") ModelState.AddModelError(nameof(model.PaymentMethod), "Phương thức thanh toán không hợp lệ.");
        if (!ModelState.IsValid) return View(model);

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var ids = model.Cart.Lines.Select(x => x.Variant.Id).ToList();
        var variants = await db.ProductVariants.Include(x => x.Product).Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        foreach (var line in model.Cart.Lines)
        {
            if (!variants.TryGetValue(line.Variant.Id, out var variant) || variant.ProductId != line.Product.Id ||
                !variant.IsActive || !variant.Product.IsActive || variant.Stock < line.Quantity)
            {
                ModelState.AddModelError("", $"{line.Product.Name} không đủ hàng. Vui lòng cập nhật giỏ hàng.");
                await transaction.RollbackAsync();
                return View(model);
            }
        }

        DiscountOffer? offer = null;
        decimal discount = 0;
        var subtotal = model.Cart.Lines.Sum(x => variants[x.Variant.Id].Price * x.Quantity);
        if (!string.IsNullOrEmpty(model.DiscountCode))
        {
            offer = await db.DiscountOffers.FromSqlInterpolated($"SELECT * FROM [dbo].[DiscountOffers] WITH (UPDLOCK, HOLDLOCK) WHERE [Code] = {model.DiscountCode}").FirstOrDefaultAsync();
            var offerError = offer is null ? "Mã ưu đãi không tồn tại." : DiscountCalculator.Validate(offer, subtotal, DateTime.UtcNow);
            if (offerError is not null)
            {
                ModelState.AddModelError(nameof(model.DiscountCode), offerError);
                await transaction.RollbackAsync();
                return View(model);
            }
            discount = DiscountCalculator.Calculate(offer!, subtotal);
            offer!.UsedCount++;
        }

        var order = new Order
        {
            Code = $"SMW{DateTime.UtcNow:yyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null,
            CustomerName = model.CustomerName.Trim(), Phone = model.Phone.Trim(), Email = model.Email.Trim().ToLowerInvariant(),
            Address = model.Address.Trim(), Note = model.Note?.Trim(), PaymentMethod = model.PaymentMethod,
            Subtotal = subtotal, DiscountCode = offer?.Code, DiscountAmount = discount, Total = subtotal - discount
        };
        foreach (var line in model.Cart.Lines)
        {
            var variant = variants[line.Variant.Id];
            variant.Stock -= line.Quantity;
            order.Items.Add(new OrderItem { ProductId = variant.ProductId, ProductVariantId = variant.Id,
                ProductName = variant.Product.Name, VariantLabel = $"{variant.ColorName} · {variant.Storage}",
                UnitPrice = variant.Price, Quantity = line.Quantity });
        }
        foreach (var product in variants.Values.Select(x => x.Product).DistinctBy(x => x.Id))
        {
            await db.Entry(product).Collection(x => x.Variants).LoadAsync();
            ProductVariantSync.Apply(product);
        }
        order.Subtotal = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        order.Total = order.Subtotal - order.DiscountAmount;
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        cart.Clear();
        return RedirectToAction(nameof(Success), new { code = order.Code });
    }

    [HttpGet("hoan-tat/{code}")]
    public IActionResult Success(string code) => View(model: code);
}
