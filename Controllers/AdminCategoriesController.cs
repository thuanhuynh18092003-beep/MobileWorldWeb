using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Controllers;

[Authorize(Roles = "Admin"), Route("quan-tri/danh-muc")]
public class AdminCategoriesController(ShopDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await db.ParentCategories.AsNoTracking()
        .Include(x => x.Subcategories).ThenInclude(x => x.Products)
        .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync());

    [HttpPost("nhom/them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddParent(string name, string slug, int displayOrder = 0)
    {
        name = name?.Trim() ?? ""; slug = slug?.Trim().ToLowerInvariant() ?? "";
        if (!Valid(name, slug) || await db.ParentCategories.AnyAsync(x => x.Slug == slug)) return Error("Tên hoặc đường dẫn nhóm không hợp lệ hoặc đã tồn tại.");
        db.ParentCategories.Add(new ParentCategory { Name = name, Slug = slug, DisplayOrder = displayOrder });
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm danh mục lớn.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("nhom/{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditParent(int id, string name, string slug, int displayOrder = 0)
    {
        var item = await db.ParentCategories.FindAsync(id);
        if (item is null) return NotFound();
        name = name?.Trim() ?? ""; slug = slug?.Trim().ToLowerInvariant() ?? "";
        if (!Valid(name, slug) || await db.ParentCategories.AnyAsync(x => x.Id != id && x.Slug == slug)) return Error("Tên hoặc đường dẫn nhóm không hợp lệ hoặc đã tồn tại.");
        item.Name = name; item.Slug = slug; item.DisplayOrder = displayOrder;
        var products = await db.Products.Where(x => x.Subcategory != null && x.Subcategory.ParentCategoryId == id).ToListAsync();
        foreach (var product in products) { product.Category = name; product.Brand = slug == "iphone" ? "Apple" : name; }
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật danh mục lớn.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("nhom/{id:int}/xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteParent(int id)
    {
        var item = await db.ParentCategories.FindAsync(id);
        if (item is null) return NotFound();
        if (await db.Subcategories.AnyAsync(x => x.ParentCategoryId == id)) return Error("Hãy chuyển hoặc xóa các danh mục nhỏ trước.");
        db.ParentCategories.Remove(item);
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa danh mục lớn.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("loai/them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddChild(int parentCategoryId, string name, string slug, int displayOrder = 0)
    {
        name = name?.Trim() ?? ""; slug = slug?.Trim().ToLowerInvariant() ?? "";
        if (!await db.ParentCategories.AnyAsync(x => x.Id == parentCategoryId) || !Valid(name, slug) ||
            await db.Subcategories.AnyAsync(x => x.ParentCategoryId == parentCategoryId && x.Slug == slug)) return Error("Danh mục lớn, tên hoặc đường dẫn không hợp lệ hoặc đã tồn tại.");
        db.Subcategories.Add(new Subcategory { ParentCategoryId = parentCategoryId, Name = name, Slug = slug, DisplayOrder = displayOrder });
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm danh mục nhỏ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("loai/{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditChild(int id, int parentCategoryId, string name, string slug, int displayOrder = 0)
    {
        var item = await db.Subcategories.FindAsync(id);
        if (item is null) return NotFound();
        name = name?.Trim() ?? ""; slug = slug?.Trim().ToLowerInvariant() ?? "";
        if (!await db.ParentCategories.AnyAsync(x => x.Id == parentCategoryId) || !Valid(name, slug) ||
            await db.Subcategories.AnyAsync(x => x.Id != id && x.ParentCategoryId == parentCategoryId && x.Slug == slug)) return Error("Danh mục lớn, tên hoặc đường dẫn không hợp lệ hoặc đã tồn tại.");
        item.ParentCategoryId = parentCategoryId; item.Name = name; item.Slug = slug; item.DisplayOrder = displayOrder;
        var parent = await db.ParentCategories.FindAsync(parentCategoryId);
        var products = await db.Products.Where(x => x.SubcategoryId == id).ToListAsync();
        foreach (var product in products) { product.Category = parent!.Name; product.Brand = parent.Slug == "iphone" ? "Apple" : parent.Name; }
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật danh mục nhỏ.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("loai/{id:int}/xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteChild(int id)
    {
        var item = await db.Subcategories.FindAsync(id);
        if (item is null) return NotFound();
        if (await db.Products.AnyAsync(x => x.SubcategoryId == id)) return Error("Hãy chuyển sản phẩm sang danh mục khác trước khi xóa.");
        db.Subcategories.Remove(item);
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa danh mục nhỏ.";
        return RedirectToAction(nameof(Index));
    }

    private static bool Valid(string name, string slug) => name.Length is > 0 and <= 80 && slug.Length is > 0 and <= 100 && Regex.IsMatch(slug, "^[a-z0-9]+(?:-[a-z0-9]+)*$");
    private IActionResult Error(string message) { TempData["Error"] = message; return RedirectToAction(nameof(Index)); }
}
