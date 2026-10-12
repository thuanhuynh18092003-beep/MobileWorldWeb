using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Services;

public class CartService(IHttpContextAccessor accessor, ShopDbContext db)
{
    private const string Key = "shop_cart";
    private ISession Session => accessor.HttpContext!.Session;
    private Dictionary<string, int> Read() => JsonSerializer.Deserialize<Dictionary<string, int>>(Session.GetString(Key) ?? "{}") ?? [];
    private void Save(Dictionary<string, int> cart) => Session.SetString(Key, JsonSerializer.Serialize(cart));
    private static string VariantKey(int id) => $"v:{id}";
    public int Count => Read().Values.Sum();
    public void Clear() => Session.Remove(Key);

    public async Task<bool> AddAsync(int productId, int? variantId, int quantity)
    {
        if (quantity < 1) return false;
        await GetAsync();
        var variants = await db.ProductVariants.AsNoTracking().Include(x => x.Product)
            .Where(x => x.ProductId == productId && x.Product.IsActive && x.IsActive && x.Stock > 0).ToListAsync();
        var variant = variantId is int selectedId ? variants.FirstOrDefault(x => x.Id == selectedId)
            : variants.FirstOrDefault(x => x.IsDefault) ?? variants.FirstOrDefault();
        if (variant is null) return false;
        var cart = Read();
        var key = VariantKey(variant.Id);
        cart[key] = Math.Min(variant.Stock, cart.GetValueOrDefault(key) + quantity);
        Save(cart);
        return true;
    }

    public async Task UpdateAsync(int variantId, int quantity)
    {
        await GetAsync();
        var cart = Read();
        var key = VariantKey(variantId);
        if (quantity <= 0) cart.Remove(key);
        else
        {
            var stock = await db.ProductVariants.Where(x => x.Id == variantId && x.IsActive && x.Product.IsActive).Select(x => x.Stock).FirstOrDefaultAsync();
            if (stock > 0) cart[key] = Math.Min(quantity, stock);
            else cart.Remove(key);
        }
        Save(cart);
    }

    public async Task<CartViewModel> GetAsync()
    {
        var cart = Read();
        var legacyIds = cart.Keys.Where(x => !x.StartsWith("v:") && int.TryParse(x, out _)).Select(int.Parse).ToList();
        if (legacyIds.Count > 0)
        {
            var defaults = await db.ProductVariants.AsNoTracking().Where(x => legacyIds.Contains(x.ProductId) && x.IsActive)
                .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id).ToListAsync();
            foreach (var legacyId in legacyIds)
            {
                var key = legacyId.ToString();
                var variant = defaults.FirstOrDefault(x => x.ProductId == legacyId);
                if (variant is not null) cart[VariantKey(variant.Id)] = cart.GetValueOrDefault(VariantKey(variant.Id)) + cart[key];
                cart.Remove(key);
            }
        }
        var ids = cart.Keys.Where(x => x.StartsWith("v:") && int.TryParse(x[2..], out _)).Select(x => int.Parse(x[2..])).ToList();
        var variants = await db.ProductVariants.AsNoTracking().Include(x => x.Product)
            .Where(x => ids.Contains(x.Id) && x.IsActive && x.Product.IsActive).ToListAsync();
        var lines = new List<CartLine>();
        var valid = new Dictionary<string, int>();
        foreach (var variant in variants)
        {
            var quantity = Math.Min(cart.GetValueOrDefault(VariantKey(variant.Id)), variant.Stock);
            if (quantity < 1) continue;
            valid[VariantKey(variant.Id)] = quantity;
            lines.Add(new CartLine { Product = variant.Product, Variant = variant, Quantity = quantity });
        }
        Save(valid);
        return new CartViewModel { Lines = lines.OrderBy(x => x.Product.Name).ThenBy(x => x.Variant.Storage).ToList() };
    }
}
