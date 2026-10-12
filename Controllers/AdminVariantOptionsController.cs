using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Authorize(Roles = "Admin"), Route("quan-tri/danh-muc-bien-the")]
public class AdminVariantOptionsController(ShopDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(new VariantOptionsViewModel
    {
        Colors = await db.VariantColors.AsNoTracking().OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync(),
        Storages = await db.VariantStorages.AsNoTracking().OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync()
    });

    [HttpPost("mau/them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddColor(string name, string hex, int displayOrder = 0)
    {
        name = name?.Trim() ?? "";
        if (!ValidColor(name, hex)) return Error("Tên màu tối đa 60 ký tự; mã màu phải có dạng #RRGGBB.");
        if (await db.VariantColors.AnyAsync(x => x.Name == name)) return Error("Màu này đã có trong danh mục.");
        db.VariantColors.Add(new VariantColor { Name = name, Hex = hex.ToUpperInvariant(), DisplayOrder = displayOrder });
        await db.SaveChangesAsync();
        return Success("Đã thêm màu vào danh mục biến thể.");
    }

    [HttpPost("mau/{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditColor(int id, string name, string hex, int displayOrder = 0)
    {
        var color = await db.VariantColors.FindAsync(id);
        if (color is null) return NotFound();
        name = name?.Trim() ?? "";
        if (!ValidColor(name, hex)) return Error("Tên màu tối đa 60 ký tự; mã màu phải có dạng #RRGGBB.");
        if (await db.VariantColors.AnyAsync(x => x.Id != id && x.Name == name)) return Error("Màu này đã có trong danh mục.");
        color.Name = name;
        color.Hex = hex.ToUpperInvariant();
        color.DisplayOrder = displayOrder;
        var productIds = await db.ProductVariants.Where(x => x.VariantColorId == id).Select(x => x.ProductId).Distinct().ToListAsync();
        var products = await db.Products.Include(x => x.Variants).Where(x => productIds.Contains(x.Id)).ToListAsync();
        foreach (var product in products)
        {
            foreach (var variant in product.Variants.Where(x => x.VariantColorId == id))
            {
                variant.ColorName = color.Name;
                variant.ColorHex = color.Hex;
            }
            ProductVariantSync.Apply(product);
        }
        await db.SaveChangesAsync();
        return Success("Đã cập nhật màu và các phiên bản đang dùng màu này.");
    }

    [HttpPost("mau/{id:int}/xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteColor(int id)
    {
        var color = await db.VariantColors.FindAsync(id);
        if (color is null) return NotFound();
        if (await db.ProductVariants.AnyAsync(x => x.VariantColorId == id)) return Error("Màu đang được sản phẩm sử dụng. Hãy chuyển các phiên bản sang màu khác trước.");
        db.VariantColors.Remove(color);
        await db.SaveChangesAsync();
        return Success("Đã xóa màu khỏi danh mục.");
    }

    [HttpPost("dung-luong/them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStorage(string name, int displayOrder = 0)
    {
        name = name?.Trim() ?? "";
        if (name.Length is < 1 or > 30) return Error("Tên dung lượng cần từ 1 đến 30 ký tự.");
        if (await db.VariantStorages.AnyAsync(x => x.Name == name)) return Error("Dung lượng này đã có trong danh mục.");
        db.VariantStorages.Add(new VariantStorage { Name = name, DisplayOrder = displayOrder });
        await db.SaveChangesAsync();
        return Success("Đã thêm dung lượng vào danh mục biến thể.");
    }

    [HttpPost("dung-luong/{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStorage(int id, string name, int displayOrder = 0)
    {
        var storage = await db.VariantStorages.FindAsync(id);
        if (storage is null) return NotFound();
        name = name?.Trim() ?? "";
        if (name.Length is < 1 or > 30) return Error("Tên dung lượng cần từ 1 đến 30 ký tự.");
        if (await db.VariantStorages.AnyAsync(x => x.Id != id && x.Name == name)) return Error("Dung lượng này đã có trong danh mục.");
        storage.Name = name;
        storage.DisplayOrder = displayOrder;
        var productIds = await db.ProductVariants.Where(x => x.VariantStorageId == id).Select(x => x.ProductId).Distinct().ToListAsync();
        var products = await db.Products.Include(x => x.Variants).Where(x => productIds.Contains(x.Id)).ToListAsync();
        foreach (var product in products)
        {
            foreach (var variant in product.Variants.Where(x => x.VariantStorageId == id)) variant.Storage = storage.Name;
            ProductVariantSync.Apply(product);
        }
        await db.SaveChangesAsync();
        return Success("Đã cập nhật dung lượng và các phiên bản đang dùng.");
    }

    [HttpPost("dung-luong/{id:int}/xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStorage(int id)
    {
        var storage = await db.VariantStorages.FindAsync(id);
        if (storage is null) return NotFound();
        if (await db.ProductVariants.AnyAsync(x => x.VariantStorageId == id)) return Error("Dung lượng đang được sản phẩm sử dụng. Hãy chuyển các phiên bản trước.");
        db.VariantStorages.Remove(storage);
        await db.SaveChangesAsync();
        return Success("Đã xóa dung lượng khỏi danh mục.");
    }

    private static bool ValidColor(string name, string hex) => name.Length is >= 1 and <= 60 &&
        Regex.IsMatch(hex ?? "", "^#[0-9a-fA-F]{6}$");

    private IActionResult Error(string message) { TempData["Error"] = message; return RedirectToAction(nameof(Index))!; }
    private IActionResult Success(string message) { TempData["Success"] = message; return RedirectToAction(nameof(Index))!; }
}
