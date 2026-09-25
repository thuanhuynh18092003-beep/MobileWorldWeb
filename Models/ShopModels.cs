using System.ComponentModel.DataAnnotations;

namespace ShopMobileWorld.Models;

public class Product
{
    public int Id { get; set; }
    [Required, StringLength(160)] public string Name { get; set; } = "";
    [Required, StringLength(180)] public string Slug { get; set; } = "";
    [Required, StringLength(80)] public string Brand { get; set; } = "";
    [Required, StringLength(80)] public string Category { get; set; } = "Điện thoại";
    [Range(0, 999999999)] public decimal Price { get; set; }
    [Range(0, 999999999)] public decimal? OldPrice { get; set; }
    [Range(0, 100000)] public int Stock { get; set; }
    [StringLength(40)] public string Color { get; set; } = "#ddd6c8";
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
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    [Required] public string ProductName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
