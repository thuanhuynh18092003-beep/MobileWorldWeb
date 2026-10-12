using Microsoft.AspNetCore.Mvc;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Route("gio-hang")]
public class CartController(CartService cart) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await cart.GetAsync());

    [HttpPost("them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int? variantId, int quantity = 1, string? returnUrl = null)
    {
        if (!await cart.AddAsync(productId, variantId, quantity)) TempData["Error"] = "Phiên bản đã hết hàng hoặc không còn bán.";
        else TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/gio-hang");
    }

    [HttpPost("cap-nhat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int variantId, int quantity)
    {
        await cart.UpdateAsync(variantId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int variantId)
    {
        await cart.UpdateAsync(variantId, 0);
        return RedirectToAction(nameof(Index));
    }
}
