using Microsoft.AspNetCore.Mvc;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Route("gio-hang")]
public class CartController(CartService cart) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await cart.GetAsync());

    [HttpPost("them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId, int quantity = 1, string? returnUrl = null)
    {
        if (!await cart.AddAsync(productId, quantity)) TempData["Error"] = "Sản phẩm đã hết hàng hoặc không còn bán.";
        else TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/gio-hang");
    }

    [HttpPost("cap-nhat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(int productId, int quantity)
    {
        await cart.UpdateAsync(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await cart.UpdateAsync(productId, 0);
        return RedirectToAction(nameof(Index));
    }
}
