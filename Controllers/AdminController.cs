using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Authorize(Roles = "Admin"), Route("quan-tri")]
public class AdminController(ShopDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewBag.ProductCount = await db.Products.CountAsync();
        ViewBag.ActiveCount = await db.Products.CountAsync(x => x.IsActive && x.Stock > 0);
        ViewBag.OutOfStockCount = await db.Products.CountAsync(x => x.Stock == 0);
        ViewBag.PendingOrderCount = await db.Orders.CountAsync(x => x.Status == "Mới đặt" || x.Status == "Đang xử lý");
        return View(await db.Products.AsNoTracking().OrderBy(x => x.Stock).ThenBy(x => x.Name).Take(10).ToListAsync());
    }

    [HttpGet("san-pham")]
    public async Task<IActionResult> Products() => View(await db.Products.AsNoTracking()
        .Include(x => x.Subcategory).ThenInclude(x => x!.ParentCategory)
        .Include(x => x.Variants)
        .OrderByDescending(x => x.Id).ToListAsync());

    [HttpGet("san-pham/them")]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View("EditProduct", new Product());
    }

    [HttpPost("san-pham/them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, int parentCategoryId)
    {
        await ValidateCategoryAsync(product, parentCategoryId);
        if (await db.Products.AnyAsync(x => x.Slug == product.Slug)) ModelState.AddModelError(nameof(product.Slug), "Đường dẫn đã tồn tại.");
        if (!ModelState.IsValid) { await LoadCategoriesAsync(); return View("EditProduct", product); }
        product.Price = 0;
        product.OldPrice = null;
        product.Stock = 0;
        product.ImageUrl = null;
        product.IsActive = false;
        product.Variants.Clear();
        db.Products.Add(product);
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã tạo sản phẩm. Hãy thêm phiên bản màu và dung lượng để bắt đầu bán.";
        return RedirectToAction("Create", "AdminVariants", new { productId = product.Id });
    }

    [HttpGet("san-pham/{id:int}/sua")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        if (product is null) return NotFound();
        await LoadCategoriesAsync();
        return View("EditProduct", product);
    }

    [HttpPost("san-pham/{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product input, int parentCategoryId)
    {
        var product = await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == id);
        if (product is null) return NotFound();
        await ValidateCategoryAsync(input, parentCategoryId);
        if (await db.Products.AnyAsync(x => x.Id != id && x.Slug == input.Slug)) ModelState.AddModelError(nameof(input.Slug), "Đường dẫn đã tồn tại.");
        if (input.IsActive && !product.Variants.Any(x => x.IsActive))
            ModelState.AddModelError(nameof(input.IsActive), "Cần ít nhất một phiên bản đang bán trước khi hiện sản phẩm.");
        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return View("EditProduct", input);
        }
        product.Name = input.Name; product.Slug = input.Slug; product.Brand = input.Brand; product.Category = input.Category;
        product.ShortDescription = input.ShortDescription; product.Description = input.Description;
        product.Screen = input.Screen; product.Camera = input.Camera; product.Battery = input.Battery; product.Chip = input.Chip;
        product.IsFeatured = input.IsFeatured; product.IsActive = input.IsActive;
        product.SubcategoryId = input.SubcategoryId;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    private async Task LoadCategoriesAsync() => ViewBag.Categories = await db.ParentCategories.AsNoTracking()
        .Include(x => x.Subcategories).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync();

    private async Task ValidateCategoryAsync(Product product, int parentCategoryId)
    {
        ModelState.Remove(nameof(Product.Brand));
        ModelState.Remove(nameof(Product.Category));
        if (product.SubcategoryId is null || !await db.Subcategories.Include(x => x.ParentCategory)
            .AnyAsync(x => x.Id == product.SubcategoryId && x.ParentCategoryId == parentCategoryId))
        {
            ModelState.AddModelError(nameof(Product.SubcategoryId), "Vui lòng chọn đúng danh mục lớn và danh mục nhỏ.");
            return;
        }
        var parent = await db.ParentCategories.FindAsync(parentCategoryId);
        product.Category = parent!.Name;
        product.Brand = parent.Slug == "iphone" ? "Apple" : parent.Name;
    }

    [HttpPost("san-pham/{id:int}/an"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (!product.IsActive && !await db.ProductVariants.AnyAsync(x => x.ProductId == id && x.IsActive))
        {
            TempData["Error"] = "Hãy thêm ít nhất một phiên bản đang bán trước khi hiện sản phẩm.";
            return RedirectToAction(nameof(Products));
        }
        product.IsActive = !product.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    [HttpPost("san-pham/{id:int}/ton-kho"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int id, int stock)
    {
        if (stock < 0 || stock > 100000) return BadRequest("Số lượng tồn kho không hợp lệ.");
        var product = await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == id);
        if (product is null) return NotFound();
        var primary = product.Variants.FirstOrDefault(x => x.IsDefault) ?? product.Variants.FirstOrDefault();
        if (primary is null) return BadRequest("Sản phẩm chưa có phiên bản.");
        primary.Stock = stock;
        ProductVariantSync.Apply(product);
        await db.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật tồn kho của {product.Name}.";
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("don-hang")]
    public async Task<IActionResult> Orders() => View(await db.Orders.AsNoTracking().Include(x => x.Items).OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpPost("don-hang/{id:int}/trang-thai"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var order = await db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();
        if (!OrderStatus.CanTransition(order.Status, status)) return BadRequest("Chuyển trạng thái không hợp lệ.");
        if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            var ids = order.Items.Select(x => x.ProductId).ToList();
            var products = await db.Products.Include(x => x.Variants).Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
            foreach (var item in order.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product)) continue;
                var variant = product.Variants.FirstOrDefault(x => x.Id == item.ProductVariantId)
                    ?? product.Variants.FirstOrDefault(x => x.IsDefault) ?? product.Variants.FirstOrDefault();
                if (variant is not null) variant.Stock += item.Quantity;
            }
            foreach (var product in products.Values) ProductVariantSync.Apply(product);
            if (!string.IsNullOrWhiteSpace(order.DiscountCode))
            {
                var offer = await db.DiscountOffers.FirstOrDefaultAsync(x => x.Code == order.DiscountCode);
                if (offer is not null && offer.UsedCount > 0) offer.UsedCount--;
            }
        }
        if (status == OrderStatus.Completed) order.CompletedAt ??= DateTime.UtcNow;
        order.Status = status;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Orders));
    }
}
