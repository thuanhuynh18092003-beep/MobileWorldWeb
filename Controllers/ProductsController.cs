using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Controllers;

[Route("dien-thoai")]
public class ProductsController(ShopDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, string? brand, string? sort, int? parentCategoryId, int? subcategoryId, int page = 1)
    {
        page = Math.Max(1, page);
        var query = db.Products.AsNoTracking().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(x => x.Name.Contains(search) || x.Brand.Contains(search));
        if (!string.IsNullOrWhiteSpace(brand)) query = query.Where(x => x.Brand == brand);
        if (parentCategoryId is > 0) query = query.Where(x => x.Subcategory != null && x.Subcategory.ParentCategoryId == parentCategoryId);
        if (subcategoryId is > 0) query = query.Where(x => x.SubcategoryId == subcategoryId);
        query = sort switch
        {
            "price-asc" => query.OrderBy(x => x.Price),
            "price-desc" => query.OrderByDescending(x => x.Price),
            "name" => query.OrderBy(x => x.Name),
            _ => query.OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.CreatedAt)
        };
        var count = await query.CountAsync();
        var model = new CatalogViewModel
        {
            Products = await query.Skip((page - 1) * 12).Take(12).ToListAsync(),
            Categories = await db.ParentCategories.AsNoTracking().Include(x => x.Subcategories).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync(),
            ParentCategoryId = parentCategoryId, SubcategoryId = subcategoryId,
            Search = search, Brand = brand, Sort = sort, Page = page, TotalCount = count,
            TotalPages = (int)Math.Ceiling(count / 12.0)
        };
        return View(model);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, int? variantId = null)
    {
        if (slug.Equals("iphone-18-pro-max-xanh", StringComparison.OrdinalIgnoreCase))
            return RedirectToActionPermanent(nameof(Details), new { slug = "iphone-18-promax" });
        var product = await db.Products.AsNoTracking().Include(x => x.Variants).FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive);
        if (product is null) return NotFound();
        ViewBag.SelectedVariantId = variantId;
        ViewBag.Related = await db.Products.AsNoTracking().Where(x => x.IsActive && x.Brand == product.Brand && x.Id != product.Id).Take(4).ToListAsync();
        return View(product);
    }
}
