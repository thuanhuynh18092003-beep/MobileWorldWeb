using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopMobileWorld.Models;

namespace ShopMobileWorld.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ShopDbContext db, IConfiguration configuration)
    {
        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(
                Phone("iPhone 16 Pro Max", "iphone-16-pro-max", "Apple", 33990000, 36990000, 22, "#bdaf9c", "256GB", "Sức mạnh Pro. Thiết kế titan tinh tế.", "6.9 inch Super Retina XDR", "48MP Fusion", "33 giờ video", "A18 Pro", true),
                Phone("Samsung Galaxy S25 Ultra", "samsung-galaxy-s25-ultra", "Samsung", 31990000, 33990000, 18, "#b5b1a5", "256GB", "Đỉnh cao sáng tạo cùng Galaxy AI.", "6.9 inch Dynamic AMOLED", "200MP", "5.000 mAh", "Snapdragon 8 Elite", true),
                Phone("iPhone 16", "iphone-16", "Apple", 21990000, 22990000, 30, "#9db6af", "128GB", "Sắc màu mới. Trải nghiệm đầy cảm hứng.", "6.1 inch Super Retina XDR", "48MP Fusion", "22 giờ video", "A18", true),
                Phone("Xiaomi 15 Ultra", "xiaomi-15-ultra", "Xiaomi", 27990000, 29990000, 14, "#292929", "512GB", "Nhiếp ảnh di động vượt mọi giới hạn.", "6.73 inch AMOLED", "Leica 50MP", "5.410 mAh", "Snapdragon 8 Elite", true),
                Phone("Samsung Galaxy Z Flip6", "samsung-galaxy-z-flip6", "Samsung", 20990000, 23990000, 11, "#c2b5cb", "256GB", "Gập mở phong cách riêng của bạn.", "6.7 inch Dynamic AMOLED", "50MP", "4.000 mAh", "Snapdragon 8 Gen 3", false),
                Phone("OPPO Find X8 Pro", "oppo-find-x8-pro", "OPPO", 21990000, 23990000, 16, "#c8c8c2", "512GB", "Chân dung Hasselblad, cảm hứng bất tận.", "6.78 inch AMOLED", "Hasselblad 50MP", "5.910 mAh", "Dimensity 9400", false),
                Phone("Google Pixel 9 Pro", "google-pixel-9-pro", "Google", 24990000, null, 9, "#d8d1c6", "256GB", "Nhiếp ảnh thông minh trong từng khoảnh khắc.", "6.3 inch LTPO OLED", "50MP", "4.700 mAh", "Tensor G4", false),
                Phone("Samsung Galaxy A56", "samsung-galaxy-a56", "Samsung", 9990000, 10990000, 27, "#b7c2ce", "128GB", "Lựa chọn cân bằng cho mỗi ngày.", "6.7 inch Super AMOLED", "50MP", "5.000 mAh", "Exynos 1580", false),
                Phone("Xiaomi Redmi Note 14 Pro", "xiaomi-redmi-note-14-pro", "Xiaomi", 7990000, 8990000, 35, "#b4b7b0", "256GB", "Mọi trải nghiệm đều thật nổi bật.", "6.67 inch AMOLED", "200MP", "5.110 mAh", "Dimensity 7300", false),
                Phone("iPhone 15", "iphone-15", "Apple", 16990000, 19990000, 21, "#d8c6bf", "128GB", "Yêu ngay từ cái nhìn đầu tiên.", "6.1 inch Super Retina XDR", "48MP", "20 giờ video", "A16 Bionic", false),
                Phone("vivo V30 5G", "vivo-v30-5g", "vivo", 11990000, 12990000, 18, "#9cc5bf", "256GB", "Chân dung rạng ngời, thiết kế mỏng nhẹ.", "6.78 inch AMOLED", "50MP ZEISS", "5.000 mAh", "Snapdragon 7 Gen 3", false),
                Phone("OPPO Reno13 F", "oppo-reno13-f", "OPPO", 8990000, 9990000, 26, "#b0c8da", "256GB", "Bền bỉ, tinh tế và luôn sẵn sàng.", "6.67 inch AMOLED", "50MP", "5.800 mAh", "Snapdragon 6 Gen 1", false)
            );
            await db.SaveChangesAsync();
        }

        await SeedCategoriesAsync(db);
        await SeedVariantsAsync(db);
        await SeedVariantOptionsAsync(db);

        var email = configuration["ADMIN_EMAIL"]?.Trim().ToLowerInvariant();
        var password = configuration["ADMIN_PASSWORD"];
        if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password) && !await db.Users.AnyAsync(x => x.Email == email))
        {
            var admin = new User { FullName = "Quản trị viên", Email = email, Role = "Admin" };
            admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedVariantsAsync(ShopDbContext db)
    {
        // Only migrate products created by the old schema. A newly created product has
        // no price until its first variant is entered in Admin.
        var products = await db.Products.Where(x => !x.Variants.Any() && x.Price > 0).ToListAsync();
        foreach (var product in products)
        {
            product.Variants.Add(new ProductVariant
            {
                ColorName = "Màu tiêu chuẩn", ColorHex = System.Text.RegularExpressions.Regex.IsMatch(product.Color, "^#[0-9a-fA-F]{6}$") ? product.Color : "#ddd6c8",
                Storage = string.IsNullOrWhiteSpace(product.Storage) ? "128GB" : product.Storage,
                Price = product.Price, OldPrice = product.OldPrice, Stock = product.Stock,
                ImageUrl = product.ImageUrl, IsDefault = true
            });
        }
        if (products.Count > 0) await db.SaveChangesAsync();
    }

    private static async Task SeedVariantOptionsAsync(ShopDbContext db)
    {
        var variants = await db.ProductVariants.Include(x => x.Product).ToListAsync();
        var colors = await db.VariantColors.ToListAsync();
        var storages = await db.VariantStorages.ToListAsync();

        // Legacy sample products only stored a generic color label while each had a
        // different color value. Give them separate catalog entries so editing one
        // sample color cannot silently recolor every other product.
        foreach (var variant in variants.Where(x => x.ColorName == "Màu tiêu chuẩn"))
        {
            var prefix = "Màu tiêu chuẩn · ";
            var suffix = $" #{variant.ProductId}";
            var maxProductName = 60 - prefix.Length - suffix.Length;
            variant.ColorName = prefix + variant.Product.Name[..Math.Min(variant.Product.Name.Length, maxProductName)] + suffix;
            variant.VariantColorId = null;
        }

        foreach (var variant in variants)
        {
            if (!colors.Any(x => x.Name.Equals(variant.ColorName, StringComparison.OrdinalIgnoreCase)))
            {
                var color = new VariantColor { Name = variant.ColorName, Hex = variant.ColorHex,
                    DisplayOrder = colors.Count };
                db.VariantColors.Add(color);
                colors.Add(color);
            }
            if (!storages.Any(x => x.Name.Equals(variant.Storage, StringComparison.OrdinalIgnoreCase)))
            {
                var storage = new VariantStorage { Name = variant.Storage, DisplayOrder = storages.Count };
                db.VariantStorages.Add(storage);
                storages.Add(storage);
            }
        }
        await db.SaveChangesAsync();

        foreach (var variant in variants)
        {
            variant.VariantColorId ??= colors.First(x => x.Name.Equals(variant.ColorName, StringComparison.OrdinalIgnoreCase)).Id;
            variant.VariantStorageId ??= storages.First(x => x.Name.Equals(variant.Storage, StringComparison.OrdinalIgnoreCase)).Id;
        }
        await db.SaveChangesAsync();

        var unusedGenericColors = await db.VariantColors
            .Where(x => x.Name == "Màu tiêu chuẩn" && !x.Variants.Any()).ToListAsync();
        if (unusedGenericColors.Count > 0)
        {
            db.VariantColors.RemoveRange(unusedGenericColors);
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedCategoriesAsync(ShopDbContext db)
    {
        var definitions = new (string Parent, string Slug, string[] Children)[]
        {
            ("iPhone", "iphone", ["iPhone 15", "iPhone 16", "iPhone 17", "iPhone 18"]),
            ("Samsung", "samsung", ["Galaxy S", "Galaxy Z", "Galaxy A"]),
            ("Xiaomi", "xiaomi", ["Xiaomi", "Redmi"]),
            ("OPPO", "oppo", ["Find", "Reno"]),
            ("Google", "google", ["Pixel"]),
            ("vivo", "vivo", ["V series"])
        };
        if (!await db.ParentCategories.AnyAsync()) foreach (var (name, slug, children) in definitions)
        {
            var parent = await db.ParentCategories.Include(x => x.Subcategories).FirstOrDefaultAsync(x => x.Slug == slug);
            if (parent is null)
            {
                parent = new ParentCategory { Name = name, Slug = slug, DisplayOrder = Array.FindIndex(definitions, x => x.Slug == slug) };
                db.ParentCategories.Add(parent);
            }
            foreach (var childName in children)
            {
                var childSlug = childName.ToLowerInvariant().Replace(' ', '-');
                if (!parent.Subcategories.Any(x => x.Slug == childSlug))
                    parent.Subcategories.Add(new Subcategory { Name = childName, Slug = childSlug, DisplayOrder = Array.IndexOf(children, childName) });
            }
        }
        await db.SaveChangesAsync();

        var parents = await db.ParentCategories.Include(x => x.Subcategories).ToListAsync();
        var products = await db.Products.Where(x => x.SubcategoryId == null).ToListAsync();
        foreach (var product in products)
        {
            var parent = parents.FirstOrDefault(x => x.Slug == (product.Brand.Equals("Apple", StringComparison.OrdinalIgnoreCase) ? "iphone" : product.Brand.ToLowerInvariant()) || x.Name.Equals(product.Brand, StringComparison.OrdinalIgnoreCase));
            if (parent is null)
            {
                parent = new ParentCategory { Name = product.Brand, Slug = $"brand-{product.Id}" };
                parent.Subcategories.Add(new Subcategory { Name = "Khác", Slug = "khac" });
                db.ParentCategories.Add(parent);
                parents.Add(parent);
            }
            var child = parent.Subcategories
                .Where(x => product.Name.Contains(x.Name, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Name.Length).FirstOrDefault();
            if (parent.Slug == "xiaomi" && product.Name.Contains("Redmi", StringComparison.OrdinalIgnoreCase))
                child = parent.Subcategories.FirstOrDefault(x => x.Slug == "redmi") ?? child;
            if (child is null && parent.Slug == "vivo" && product.Name.StartsWith("vivo V", StringComparison.OrdinalIgnoreCase))
                child = parent.Subcategories.FirstOrDefault(x => x.Slug == "v-series");
            if (child is null)
            {
                child = parent.Subcategories.FirstOrDefault(x => x.Slug == "khac");
                if (child is null) { child = new Subcategory { Name = "Khác", Slug = "khac" }; parent.Subcategories.Add(child); }
            }
            product.Subcategory = child;
        }
        await db.SaveChangesAsync();
    }

    private static Product Phone(string name, string slug, string brand, decimal price, decimal? oldPrice, int stock, string color, string storage, string shortDescription, string screen, string camera, string battery, string chip, bool featured) => new()
    {
        Name = name, Slug = slug, Brand = brand, Price = price, OldPrice = oldPrice, Stock = stock,
        Color = color, Storage = storage, ShortDescription = shortDescription,
        Description = $"{name} mang đến trải nghiệm cao cấp với thiết kế hiện đại, hiệu năng mượt mà và hệ thống camera ấn tượng. Sản phẩm chính hãng, bảo hành 12 tháng tại ShopMobileWorld.",
        Screen = screen, Camera = camera, Battery = battery, Chip = chip, IsFeatured = featured
    };
}
