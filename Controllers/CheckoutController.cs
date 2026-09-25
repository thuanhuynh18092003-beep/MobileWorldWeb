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
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        model.Cart = await cart.GetAsync();
        if (model.Cart.Count == 0) return RedirectToAction("Index", "Cart");
        if (model.PaymentMethod != "COD" && model.PaymentMethod != "BankTransfer") ModelState.AddModelError(nameof(model.PaymentMethod), "Phương thức thanh toán không hợp lệ.");
        if (!ModelState.IsValid) return View(model);

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var ids = model.Cart.Lines.Select(x => x.Product.Id).ToList();
        var products = await db.Products.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        foreach (var line in model.Cart.Lines)
        {
            if (!products.TryGetValue(line.Product.Id, out var p) || !p.IsActive || p.Stock < line.Quantity)
            {
                ModelState.AddModelError("", $"{line.Product.Name} không đủ hàng. Vui lòng cập nhật giỏ hàng.");
                await transaction.RollbackAsync();
                return View(model);
            }
        }

        var order = new Order
        {
            Code = $"SMW{DateTime.UtcNow:yyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            UserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null,
            CustomerName = model.CustomerName.Trim(), Phone = model.Phone.Trim(), Email = model.Email.Trim().ToLowerInvariant(),
            Address = model.Address.Trim(), Note = model.Note?.Trim(), PaymentMethod = model.PaymentMethod,
            Total = model.Cart.Total
        };
        foreach (var line in model.Cart.Lines)
        {
            var p = products[line.Product.Id];
            p.Stock -= line.Quantity;
            order.Items.Add(new OrderItem { ProductId = p.Id, ProductName = p.Name, UnitPrice = p.Price, Quantity = line.Quantity });
        }
        order.Total = order.Items.Sum(x => x.UnitPrice * x.Quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        cart.Clear();
        return RedirectToAction(nameof(Success), new { code = order.Code });
    }

    [HttpGet("hoan-tat/{code}")]
    public IActionResult Success(string code) => View(model: code);
}
