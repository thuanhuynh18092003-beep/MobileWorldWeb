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

    private static Product Phone(string name, string slug, string brand, decimal price, decimal? oldPrice, int stock, string color, string storage, string shortDescription, string screen, string camera, string battery, string chip, bool featured) => new()
    {
        Name = name, Slug = slug, Brand = brand, Price = price, OldPrice = oldPrice, Stock = stock,
        Color = color, Storage = storage, ShortDescription = shortDescription,
        Description = $"{name} mang đến trải nghiệm cao cấp với thiết kế hiện đại, hiệu năng mượt mà và hệ thống camera ấn tượng. Sản phẩm chính hãng, bảo hành 12 tháng tại ShopMobileWorld.",
        Screen = screen, Camera = camera, Battery = battery, Chip = chip, IsFeatured = featured
    };
}
