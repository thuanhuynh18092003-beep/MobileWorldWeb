using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;
using ShopMobileWorld.Services;

namespace ShopMobileWorld.Controllers;

[Route("quan-tri/thiet-lap")]
public class AdminSetupController(ShopDbContext db, IWebHostEnvironment environment, CartService cart) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        if (!AdminSetupAccess.IsAllowed(HttpContext, environment)) return NotFound();
        if (await db.Users.AnyAsync(x => x.Role == "Admin"))
            return User.IsInRole("Admin") ? RedirectToAction("Index", "Admin") : NotFound();
        return View(new AdminSetupViewModel());
    }

    [HttpPost(""), ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AdminSetupViewModel model)
    {
        if (!AdminSetupAccess.IsAllowed(HttpContext, environment)) return NotFound();
        if (!ModelState.IsValid) return View(model);

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (await db.Users.AnyAsync(x => x.Role == "Admin")) return NotFound();

        var email = model.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);
        var hasher = new PasswordHasher<User>();
        if (user is null)
        {
            user = new User { FullName = model.FullName.Trim(), Email = email, Role = "Admin" };
            user.PasswordHash = hasher.HashPassword(user, model.Password);
            db.Users.Add(user);
        }
        else
        {
            if (hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.Password), "Tài khoản đã tồn tại; mật khẩu không đúng.");
                return View(model);
            }
            user.Role = "Admin";
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        cart.Clear();
        return RedirectToAction("Index", "Admin");
    }
}
