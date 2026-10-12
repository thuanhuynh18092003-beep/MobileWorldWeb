using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Authorize(Roles = "Admin"), Route("quan-tri/san-pham/{productId:int}/phien-ban")]
public class AdminVariantsController(ShopDbContext db, ProductImageStorage images) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int productId)
    {
        var product = await db.Products.AsNoTracking().Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == productId);
        return product is null ? NotFound() : View(product);
    }

    [HttpGet("them")]
    public async Task<IActionResult> Create(int productId)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null) return NotFound();
        ViewBag.Product = product;
        await LoadOptionsAsync();
        return View("Edit", new VariantEditViewModel { ProductId = productId, Price = product.Price,
            IsDefault = !await db.ProductVariants.AnyAsync(x => x.ProductId == productId) });
    }

    [HttpPost("them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int productId, VariantEditViewModel input, IFormFile? imageFile)
    {
        var product = await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null) return NotFound();
        input.ProductId = productId;
        if (product.Variants.Count == 0) input.IsDefault = true;
        var (color, storage) = await ValidateAsync(input, null, product);
        var imageError = await images.ValidateAsync(imageFile);
        if (imageError is not null) ModelState.AddModelError(nameof(imageFile), imageError);
        if (!ModelState.IsValid) { ViewBag.Product = product; await LoadOptionsAsync(); return View("Edit", input); }
        var imageUrl = imageFile is null ? product.ImageUrl : await images.SaveAsync(imageFile);
        if (input.IsDefault) foreach (var other in product.Variants) other.IsDefault = false;
        var variant = new ProductVariant { ProductId = productId, VariantColorId = color!.Id, VariantStorageId = storage!.Id,
            ColorName = color.Name, ColorHex = color.Hex, Storage = storage.Name,
            Price = input.Price, OldPrice = input.OldPrice, Stock = input.Stock,
            ImageUrl = imageUrl, IsActive = input.IsActive, IsDefault = input.IsDefault };
        product.Variants.Add(variant);
        if (product.Variants.Count == 1 && input.IsActive) product.IsActive = true;
        ProductVariantSync.Apply(product);
        try { await db.SaveChangesAsync(); }
        catch { if (imageFile is not null) images.Delete(imageUrl); throw; }
        TempData["Success"] = "Đã thêm phiên bản mới.";
        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpGet("{id:int}/sua")]
    public async Task<IActionResult> Edit(int productId, int id)
    {
        var variant = await db.ProductVariants.AsNoTracking().Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == id && x.ProductId == productId);
        if (variant is null) return NotFound();
        ViewBag.Product = variant.Product;
        await LoadOptionsAsync();
        return View(new VariantEditViewModel { Id = id, ProductId = productId,
            VariantColorId = variant.VariantColorId ?? await db.VariantColors.Where(x => x.Name == variant.ColorName).Select(x => x.Id).FirstOrDefaultAsync(),
            VariantStorageId = variant.VariantStorageId ?? await db.VariantStorages.Where(x => x.Name == variant.Storage).Select(x => x.Id).FirstOrDefaultAsync(),
            Price = variant.Price, OldPrice = variant.OldPrice, Stock = variant.Stock,
            ImageUrl = variant.ImageUrl, IsActive = variant.IsActive, IsDefault = variant.IsDefault });
    }

    [HttpPost("{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int productId, int id, VariantEditViewModel input, IFormFile? imageFile, bool removeImage = false)
    {
        var product = await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == productId);
        var variant = product?.Variants.FirstOrDefault(x => x.Id == id);
        if (variant is null) return NotFound();
        input.Id = id; input.ProductId = productId;
        var (color, storage) = await ValidateAsync(input, id, product!);
        if (variant.IsDefault && !input.IsDefault && !product!.Variants.Any(x => x.Id != id && x.IsDefault))
            ModelState.AddModelError(nameof(input.IsDefault), "Hãy chọn một phiên bản mặc định khác trước.");
        var imageError = await images.ValidateAsync(imageFile);
        if (imageError is not null) ModelState.AddModelError(nameof(imageFile), imageError);
        if (!ModelState.IsValid) { input.ImageUrl = variant.ImageUrl; ViewBag.Product = product; await LoadOptionsAsync(); return View(input); }
        var oldImage = variant.ImageUrl;
        var newImage = imageFile is null ? null : await images.SaveAsync(imageFile);
        if (input.IsDefault) foreach (var other in product!.Variants) other.IsDefault = false;
        variant.VariantColorId = color!.Id; variant.VariantStorageId = storage!.Id;
        variant.ColorName = color.Name; variant.ColorHex = color.Hex; variant.Storage = storage.Name;
        variant.Price = input.Price; variant.OldPrice = input.OldPrice; variant.Stock = input.Stock;
        variant.IsActive = input.IsActive; variant.IsDefault = input.IsDefault;
        if (variant.IsActive && !product!.IsActive && !product.Variants.Any(x => x.Id != id && x.IsActive))
            product.IsActive = true;
        if (newImage is not null) variant.ImageUrl = newImage;
        else if (removeImage) variant.ImageUrl = null;
        ProductVariantSync.Apply(product!);
        try { await db.SaveChangesAsync(); }
        catch { images.Delete(newImage); throw; }
        if (oldImage != variant.ImageUrl && !await db.ProductVariants.AnyAsync(x => x.ImageUrl == oldImage) &&
            !await db.Products.AnyAsync(x => x.ImageUrl == oldImage)) images.Delete(oldImage);
        TempData["Success"] = "Đã cập nhật phiên bản.";
        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpPost("{id:int}/trang-thai"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int productId, int id)
    {
        var product = await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == productId);
        var variant = product?.Variants.FirstOrDefault(x => x.Id == id);
        if (variant is null) return NotFound();
        variant.IsActive = !variant.IsActive;
        if (variant.IsActive && !product!.Variants.Any(x => x.Id != id && x.IsActive))
        {
            variant.IsDefault = true;
            product.IsActive = true;
        }
        if (!variant.IsActive && variant.IsDefault)
        {
            variant.IsDefault = false;
            var replacement = product!.Variants.FirstOrDefault(x => x.Id != id && x.IsActive);
            if (replacement is not null) replacement.IsDefault = true;
        }
        if (!product!.Variants.Any(x => x.IsActive)) product.IsActive = false;
        ProductVariantSync.Apply(product!);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { productId });
    }

    [HttpPost("{id:int}/xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int productId, int id)
    {
        var product = await db.Products.Include(x => x.Variants).FirstOrDefaultAsync(x => x.Id == productId);
        var variant = product?.Variants.FirstOrDefault(x => x.Id == id);
        if (variant is null) return NotFound();
        if (product!.Variants.Count == 1 || await db.OrderItems.AnyAsync(x => x.ProductVariantId == id))
        {
            TempData["Error"] = "Không thể xóa phiên bản cuối cùng hoặc phiên bản đã có trong đơn hàng. Hãy ẩn phiên bản này.";
            return RedirectToAction(nameof(Index), new { productId });
        }
        var oldImage = variant.ImageUrl;
        product.Variants.Remove(variant);
        db.ProductVariants.Remove(variant);
        if (variant.IsDefault)
        {
            var replacement = product.Variants.FirstOrDefault(x => x.IsActive) ?? product.Variants.FirstOrDefault();
            if (replacement is not null) replacement.IsDefault = true;
        }
        if (!product.Variants.Any(x => x.IsActive)) product.IsActive = false;
        ProductVariantSync.Apply(product);
        await db.SaveChangesAsync();
        if (!await db.ProductVariants.AnyAsync(x => x.ImageUrl == oldImage) && !await db.Products.AnyAsync(x => x.ImageUrl == oldImage)) images.Delete(oldImage);
        TempData["Success"] = "Đã xóa phiên bản.";
        return RedirectToAction(nameof(Index), new { productId });
    }

    private async Task LoadOptionsAsync()
    {
        ViewBag.Colors = await db.VariantColors.AsNoTracking().OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync();
        ViewBag.Storages = await db.VariantStorages.AsNoTracking().OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync();
    }

    private async Task<(VariantColor? Color, VariantStorage? Storage)> ValidateAsync(VariantEditViewModel input, int? excludingId, Product product)
    {
        var color = await db.VariantColors.FindAsync(input.VariantColorId);
        var storage = await db.VariantStorages.FindAsync(input.VariantStorageId);
        if (color is null) ModelState.AddModelError(nameof(input.VariantColorId), "Chọn màu từ danh mục biến thể.");
        if (storage is null) ModelState.AddModelError(nameof(input.VariantStorageId), "Chọn dung lượng từ danh mục biến thể.");
        if (input.IsDefault && !input.IsActive) ModelState.AddModelError(nameof(input.IsDefault), "Phiên bản mặc định cần được bật bán.");
        if (color is not null && storage is not null && product.Variants.Any(x => x.Id != excludingId &&
            x.ColorName.Equals(color.Name, StringComparison.OrdinalIgnoreCase) &&
            x.Storage.Equals(storage.Name, StringComparison.OrdinalIgnoreCase)))
            ModelState.AddModelError(nameof(input.VariantColorId), "Tổ hợp màu và dung lượng này đã có trong sản phẩm.");
        return (color, storage);
    }
}
