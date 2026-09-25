using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Controllers;

public class HomeController(ShopDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await db.Products.AsNoTracking().Where(x => x.IsActive).OrderByDescending(x => x.IsFeatured).ThenByDescending(x => x.CreatedAt).Take(8).ToListAsync();
        return View(products);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
