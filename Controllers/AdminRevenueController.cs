using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Controllers;

[Authorize(Roles = "Admin"), Route("quan-tri/doanh-thu")]
public class AdminRevenueController(ShopDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int days = 30)
    {
        if (days is not (7 or 30 or 90)) days = 30;
        var today = DateTime.Today;
        var start = today.AddDays(1 - days);
        var previousStart = start.AddDays(-days);
        var lowerUtc = previousStart.ToUniversalTime().AddDays(-1);
        var orders = await db.Orders.AsNoTracking()
            .Where(x => x.CreatedAt >= lowerUtc || x.CompletedAt >= lowerUtc)
            .Select(x => new { x.CreatedAt, x.CompletedAt, x.Status, x.Total }).ToListAsync();

        var model = new RevenueViewModel { Days = days };
        for (var i = 0; i < days; i++) model.DaysData.Add(new RevenueDay { Date = start.AddDays(i) });
        var byDay = model.DaysData.ToDictionary(x => x.Date);
        foreach (var order in orders)
        {
            var createdDay = order.CreatedAt.ToLocalTime().Date;
            if (byDay.TryGetValue(createdDay, out var created)) created.NewOrders++;
            if (order.Status != OrderStatus.Completed || order.CompletedAt is null) continue;
            var completedDay = order.CompletedAt.Value.ToLocalTime().Date;
            if (byDay.TryGetValue(completedDay, out var completed))
            {
                completed.CompletedOrders++;
                completed.Revenue += order.Total;
            }
            else if (completedDay >= previousStart && completedDay < start) model.PreviousRevenue += order.Total;
        }
        return View(model);
    }
}
