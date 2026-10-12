using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Controllers;

[Authorize(Roles = "Admin"), Route("quan-tri/uu-dai")]
public class AdminOffersController(ShopDbContext db) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View(await db.DiscountOffers.AsNoTracking().OrderByDescending(x => x.Id).ToListAsync());

    [HttpGet("them")]
    public IActionResult Create() => View("Edit", new DiscountOffer());

    [HttpPost("them"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiscountOffer input)
    {
        Normalize(input);
        Validate(input);
        if (await db.DiscountOffers.AnyAsync(x => x.Code == input.Code)) ModelState.AddModelError(nameof(input.Code), "Mã này đã tồn tại.");
        if (!ModelState.IsValid) return View("Edit", input);
        db.DiscountOffers.Add(Copy(input));
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã tạo mã ưu đãi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/sua")]
    public async Task<IActionResult> Edit(int id)
    {
        var offer = await db.DiscountOffers.FindAsync(id);
        return offer is null ? NotFound() : View(offer);
    }

    [HttpPost("{id:int}/sua"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DiscountOffer input)
    {
        var offer = await db.DiscountOffers.FindAsync(id);
        if (offer is null) return NotFound();
        Normalize(input);
        Validate(input);
        if (input.Code != offer.Code && await db.Orders.AnyAsync(x => x.DiscountCode == offer.Code))
            ModelState.AddModelError(nameof(input.Code), "Mã đã dùng trong đơn hàng nên không thể đổi. Bạn có thể sửa mức giảm hoặc tạo mã mới.");
        if (input.UsageLimit is int limit && limit < offer.UsedCount) ModelState.AddModelError(nameof(input.UsageLimit), "Giới hạn không thể ít hơn lượt đã sử dụng.");
        if (await db.DiscountOffers.AnyAsync(x => x.Id != id && x.Code == input.Code)) ModelState.AddModelError(nameof(input.Code), "Mã này đã tồn tại.");
        if (!ModelState.IsValid) { input.UsedCount = offer.UsedCount; return View(input); }
        offer.Code = input.Code; offer.Name = input.Name; offer.Type = input.Type; offer.Value = input.Value;
        offer.MinimumOrder = input.MinimumOrder; offer.MaximumDiscount = input.MaximumDiscount;
        offer.StartsAt = input.StartsAt; offer.EndsAt = input.EndsAt; offer.UsageLimit = input.UsageLimit; offer.IsActive = input.IsActive;
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật ưu đãi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/trang-thai"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var offer = await db.DiscountOffers.FindAsync(id);
        if (offer is null) return NotFound();
        offer.IsActive = !offer.IsActive;
        await db.SaveChangesAsync();
        TempData["Success"] = offer.IsActive ? "Đã bật ưu đãi." : "Đã tắt ưu đãi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/xoa"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var offer = await db.DiscountOffers.FindAsync(id);
        if (offer is null) return NotFound();
        if (await db.Orders.AnyAsync(x => x.DiscountCode == offer.Code))
        {
            TempData["Error"] = "Mã đã gắn với đơn hàng. Hãy tắt mã để giữ lịch sử.";
            return RedirectToAction(nameof(Index));
        }
        db.DiscountOffers.Remove(offer);
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa ưu đãi.";
        return RedirectToAction(nameof(Index));
    }

    private void Validate(DiscountOffer item)
    {
        if (!Regex.IsMatch(item.Code, "^[A-Z0-9_-]{3,30}$")) ModelState.AddModelError(nameof(item.Code), "Mã dài 3–30 ký tự, chỉ dùng chữ in hoa, số, _ hoặc -." );
        if (item.Name.Length is < 1 or > 120) ModelState.AddModelError(nameof(item.Name), "Vui lòng nhập tên ưu đãi.");
        if (item.Type is not ("Percent" or "Fixed")) ModelState.AddModelError(nameof(item.Type), "Loại ưu đãi không hợp lệ.");
        if (item.Value <= 0 || (item.Type == "Percent" && item.Value > 100)) ModelState.AddModelError(nameof(item.Value), "Mức giảm phải lớn hơn 0; phần trăm tối đa 100.");
        if (item.MinimumOrder < 0) ModelState.AddModelError(nameof(item.MinimumOrder), "Giá trị đơn tối thiểu không hợp lệ.");
        if (item.MaximumDiscount is <= 0) ModelState.AddModelError(nameof(item.MaximumDiscount), "Mức giảm tối đa phải lớn hơn 0.");
        if (item.EndsAt <= item.StartsAt) ModelState.AddModelError(nameof(item.EndsAt), "Ngày kết thúc phải sau ngày bắt đầu.");
        if (item.UsageLimit is <= 0) ModelState.AddModelError(nameof(item.UsageLimit), "Giới hạn lượt dùng phải lớn hơn 0.");
    }

    private static void Normalize(DiscountOffer item)
    {
        item.Code = item.Code?.Trim().ToUpperInvariant() ?? "";
        item.Name = item.Name?.Trim() ?? "";
        item.StartsAt = DateTime.SpecifyKind(item.StartsAt, DateTimeKind.Local).ToUniversalTime();
        item.EndsAt = DateTime.SpecifyKind(item.EndsAt, DateTimeKind.Local).ToUniversalTime();
    }

    private static DiscountOffer Copy(DiscountOffer item) => new()
    {
        Code = item.Code, Name = item.Name, Type = item.Type, Value = item.Value,
        MinimumOrder = item.MinimumOrder, MaximumDiscount = item.MaximumDiscount,
        StartsAt = item.StartsAt, EndsAt = item.EndsAt, UsageLimit = item.UsageLimit, IsActive = item.IsActive
    };
}
