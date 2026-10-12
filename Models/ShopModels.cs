using System.ComponentModel.DataAnnotations;

namespace ShopMobileWorld.Models;

public class Product
{
    public int Id { get; set; }
    [Required, StringLength(160)] public string Name { get; set; } = "";
    [Required, StringLength(180)] public string Slug { get; set; } = "";
    [Required, StringLength(80)] public string Brand { get; set; } = "";
    [Required, StringLength(80)] public string Category { get; set; } = "Điện thoại";
    public int? SubcategoryId { get; set; }
    public Subcategory? Subcategory { get; set; }
    [Range(0, 999999999)] public decimal Price { get; set; }
    [Range(0, 999999999)] public decimal? OldPrice { get; set; }
    [Range(0, 100000)] public int Stock { get; set; }
    [StringLength(40)] public string Color { get; set; } = "#ddd6c8";
    [StringLength(300)] public string? ImageUrl { get; set; }
    [StringLength(30)] public string Storage { get; set; } = "128GB";
    [StringLength(500)] public string ShortDescription { get; set; } = "";
    public string Description { get; set; } = "";
    [StringLength(120)] public string Screen { get; set; } = "";
    [StringLength(120)] public string Camera { get; set; } = "";
    [StringLength(120)] public string Battery { get; set; } = "";
    [StringLength(120)] public string Chip { get; set; } = "";
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ProductVariant> Variants { get; set; } = [];
}

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    [Required, StringLength(60)] public string ColorName { get; set; } = "";
    [Required, StringLength(7)] public string ColorHex { get; set; } = "#ddd6c8";
    [Required, StringLength(30)] public string Storage { get; set; } = "128GB";
    public int? VariantColorId { get; set; }
    public VariantColor? VariantColor { get; set; }
    public int? VariantStorageId { get; set; }
    public VariantStorage? VariantStorage { get; set; }
    [Range(0, 999999999)] public decimal Price { get; set; }
    [Range(0, 999999999)] public decimal? OldPrice { get; set; }
    [Range(0, 100000)] public int Stock { get; set; }
    [StringLength(300)] public string? ImageUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class VariantColor
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string Name { get; set; } = "";
    [Required, RegularExpression("^#[0-9a-fA-F]{6}$"), StringLength(7)] public string Hex { get; set; } = "#ddd6c8";
    public int DisplayOrder { get; set; }
    public List<ProductVariant> Variants { get; set; } = [];
}

public class VariantStorage
{
    public int Id { get; set; }
    [Required, StringLength(30)] public string Name { get; set; } = "";
    public int DisplayOrder { get; set; }
    public List<ProductVariant> Variants { get; set; } = [];
}

public class ParentCategory
{
    public int Id { get; set; }
    [Required, StringLength(80)] public string Name { get; set; } = "";
    [Required, StringLength(100)] public string Slug { get; set; } = "";
    public int DisplayOrder { get; set; }
    public List<Subcategory> Subcategories { get; set; } = [];
}

public class Subcategory
{
    public int Id { get; set; }
    public int ParentCategoryId { get; set; }
    public ParentCategory ParentCategory { get; set; } = null!;
    [Required, StringLength(80)] public string Name { get; set; } = "";
    [Required, StringLength(100)] public string Slug { get; set; } = "";
    public int DisplayOrder { get; set; }
    public List<Product> Products { get; set; } = [];
}

public class DiscountOffer
{
    public int Id { get; set; }
    [Required, StringLength(30)] public string Code { get; set; } = "";
    [Required, StringLength(120)] public string Name { get; set; } = "";
    [Required, StringLength(10)] public string Type { get; set; } = "Percent";
    public decimal Value { get; set; }
    public decimal MinimumOrder { get; set; }
    public decimal? MaximumDiscount { get; set; }
    public DateTime StartsAt { get; set; } = DateTime.UtcNow;
    public DateTime EndsAt { get; set; } = DateTime.UtcNow.AddMonths(1);
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;
}

public class User
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    [Required, StringLength(20)] public string Role { get; set; } = "Customer";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Order
{
    public int Id { get; set; }
    [Required, StringLength(30)] public string Code { get; set; } = "";
    public int? UserId { get; set; }
    public User? User { get; set; }
    [Required, StringLength(100)] public string CustomerName { get; set; } = "";
    [Required, StringLength(20)] public string Phone { get; set; } = "";
    [Required, EmailAddress, StringLength(200)] public string Email { get; set; } = "";
    [Required, StringLength(300)] public string Address { get; set; } = "";
    [StringLength(500)] public string? Note { get; set; }
    [Required, StringLength(30)] public string PaymentMethod { get; set; } = "COD";
    [Required, StringLength(30)] public string Status { get; set; } = "Mới đặt";
    public decimal Total { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    [StringLength(30)] public string? DiscountCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    [StringLength(100)] public string? VariantLabel { get; set; }
    [Required] public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
