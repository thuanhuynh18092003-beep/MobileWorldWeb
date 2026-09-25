using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

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
    public async Task<IActionResult> Products() => View(await db.Products.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync());

    [HttpGet("san-pham/them")]
    public IActionResult Create() => View("EditProduct", new Product());

    [HttpPost("san-pham/them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (await db.Products.AnyAsync(x => x.Slug == product.Slug)) ModelState.AddModelError(nameof(product.Slug), "Đường dẫn đã tồn tại.");
        if (!ModelState.IsValid) return View("EditProduct", product);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    [HttpGet("san-pham/{id:int}/sua")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : View("EditProduct", product);
    }

    [HttpPost("san-pham/{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product input)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (await db.Products.AnyAsync(x => x.Id != id && x.Slug == input.Slug)) ModelState.AddModelError(nameof(input.Slug), "Đường dẫn đã tồn tại.");
        if (!ModelState.IsValid) return View("EditProduct", input);
        product.Name = input.Name; product.Slug = input.Slug; product.Brand = input.Brand; product.Category = input.Category;
        product.Price = input.Price; product.OldPrice = input.OldPrice; product.Stock = input.Stock; product.Color = input.Color;
        product.Storage = input.Storage; product.ShortDescription = input.ShortDescription; product.Description = input.Description;
        product.Screen = input.Screen; product.Camera = input.Camera; product.Battery = input.Battery; product.Chip = input.Chip;
        product.IsFeatured = input.IsFeatured; product.IsActive = input.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    [HttpPost("san-pham/{id:int}/an"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        product.IsActive = !product.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Products));
    }

    [HttpPost("san-pham/{id:int}/ton-kho"), ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStock(int id, int stock)
    {
        if (stock < 0 || stock > 100000) return BadRequest("Số lượng tồn kho không hợp lệ.");
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        product.Stock = stock;
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
            var products = await db.Products.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
            foreach (var item in order.Items) if (products.TryGetValue(item.ProductId, out var product)) product.Stock += item.Quantity;
        }
        order.Status = status;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Orders));
    }
}
