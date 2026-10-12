using ShopMobileWorld.Models;

namespace ShopMobileWorld.Services;

public static class DiscountCalculator
{
    public static string? Validate(DiscountOffer offer, decimal subtotal, DateTime nowUtc)
    {
        if (!offer.IsActive) return "Mã ưu đãi đã ngừng áp dụng.";
        if (nowUtc < offer.StartsAt || nowUtc > offer.EndsAt) return "Mã ưu đãi chưa đến hạn hoặc đã hết hạn.";
        if (offer.UsageLimit is int limit && offer.UsedCount >= limit) return "Mã ưu đãi đã hết lượt sử dụng.";
        if (subtotal < offer.MinimumOrder) return $"Đơn hàng cần đạt ít nhất {offer.MinimumOrder:N0}₫ để dùng mã này.";
        return null;
    }

    public static decimal Calculate(DiscountOffer offer, decimal subtotal)
    {
        var discount = offer.Type == "Percent" ? Math.Round(subtotal * offer.Value / 100, 0, MidpointRounding.AwayFromZero) : offer.Value;
        if (offer.MaximumDiscount is decimal cap) discount = Math.Min(discount, cap);
        return Math.Clamp(discount, 0, subtotal);
    }
}
