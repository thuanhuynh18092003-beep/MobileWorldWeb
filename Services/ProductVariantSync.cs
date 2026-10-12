using ShopMobileWorld.Models;

namespace ShopMobileWorld.Services;

public static class ProductVariantSync
{
    public static void Apply(Product product)
    {
        var available = product.Variants.Where(x => x.IsActive).ToList();
        product.Stock = available.Sum(x => x.Stock);
        var display = available.FirstOrDefault(x => x.IsDefault && x.Stock > 0)
            ?? available.FirstOrDefault(x => x.Stock > 0)
            ?? available.FirstOrDefault(x => x.IsDefault)
            ?? available.FirstOrDefault()
            ?? product.Variants.FirstOrDefault(x => x.IsDefault)
            ?? product.Variants.FirstOrDefault();
        if (display is null) return;
        product.Price = display.Price;
        product.OldPrice = display.OldPrice;
        product.Storage = display.Storage;
        product.Color = display.ColorHex;
        product.ImageUrl = display.ImageUrl ?? available.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ImageUrl))?.ImageUrl;
    }
}
