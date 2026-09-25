using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Data;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Services;

public class CartService(IHttpContextAccessor accessor, ShopDbContext db)
{
    private const string Key = "shop_cart";
    private ISession Session => accessor.HttpContext!.Session;
    private Dictionary<int, int> Read() => JsonSerializer.Deserialize<Dictionary<int, int>>(Session.GetString(Key) ?? "{}") ?? [];
    private void Save(Dictionary<int, int> cart) => Session.SetString(Key, JsonSerializer.Serialize(cart));
    public int Count => Read().Values.Sum();
    public void Clear() => Session.Remove(Key);

    public async Task<bool> AddAsync(int productId, int quantity)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId && x.IsActive);
        if (product is null || product.Stock < 1 || quantity < 1) return false;
        var cart = Read();
        cart[productId] = Math.Min(product.Stock, cart.GetValueOrDefault(productId) + quantity);
        Save(cart);
        return true;
    }

    public async Task UpdateAsync(int productId, int quantity)
    {
        var cart = Read();
        if (quantity <= 0) cart.Remove(productId);
        else
        {
            var stock = await db.Products.Where(x => x.Id == productId && x.IsActive).Select(x => x.Stock).FirstOrDefaultAsync();
            if (stock > 0) cart[productId] = Math.Min(quantity, stock);
            else cart.Remove(productId);
        }
        Save(cart);
    }

    public async Task<CartViewModel> GetAsync()
    {
        var cart = Read();
        var products = await db.Products.AsNoTracking().Where(x => cart.Keys.Contains(x.Id) && x.IsActive).ToListAsync();
        return new CartViewModel { Lines = products.Select(p => new CartLine { Product = p, Quantity = Math.Min(cart[p.Id], p.Stock) }).Where(x => x.Quantity > 0).ToList() };
    }
}
