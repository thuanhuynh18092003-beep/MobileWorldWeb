using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Route("tai-khoan")]
public class AccountController(ShopDbContext db, CartService cart) : Controller
{
    [HttpGet("dang-ky")]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost("dang-ky"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");
            return View(model);
        }
        var user = new User { FullName = model.FullName.Trim(), Email = email };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, model.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        await SignInAsync(user);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("dang-nhap")]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost("dang-nhap"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user is null || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(model);
        }
        await SignInAsync(user);
        if (user.Role == "Admin")
        {
            cart.Clear();
            return RedirectToAction("Index", "Admin");
        }
        return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : "/");
    }

    [HttpPost("dang-xuat"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize, HttpGet("don-hang")]
    public async Task<IActionResult> Orders()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return View(await db.Orders.AsNoTracking().Include(x => x.Items).Where(x => x.UserId == id).OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    private async Task SignInAsync(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
    }
}
