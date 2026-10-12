using System.ComponentModel.DataAnnotations;

namespace ShopMobileWorld.Models;

public class CatalogViewModel
{
    public List<Product> Products { get; set; } = [];
    public List<ParentCategory> Categories { get; set; } = [];
    public int? ParentCategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public string? Search { get; set; }
    public string? Brand { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

public class CartLine
{
    public Product Product { get; set; } = null!;
    public ProductVariant Variant { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Total => Variant.Price * Quantity;
}

public class CartViewModel
{
    public List<CartLine> Lines { get; set; } = [];
    public decimal Total => Lines.Sum(x => x.Total);
    public int Count => Lines.Sum(x => x.Quantity);
}

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)] public string CustomerName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại"), RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại cần có 10 chữ số và bắt đầu bằng 0")] public string Phone { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ")] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng"), StringLength(300)] public string Address { get; set; } = "";
    [StringLength(500)] public string? Note { get; set; }
    [Required] public string PaymentMethod { get; set; } = "COD";
    [StringLength(30)] public string? DiscountCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PayableTotal => Math.Max(0, Cart.Total - DiscountAmount);
    public CartViewModel Cart { get; set; } = new();
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")] public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), MinLength(8, ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự")] public string Password { get; set; } = "";
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")] public string ConfirmPassword { get; set; } = "";
}

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class AdminSetupViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên"), StringLength(100)] public string FullName { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập email"), EmailAddress] public string Email { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu"), MinLength(8, ErrorMessage = "Mật khẩu cần ít nhất 8 ký tự")] public string Password { get; set; } = "";
    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu"), Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")] public string ConfirmPassword { get; set; } = "";
}

public class VariantEditViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Chọn màu trong danh mục biến thể")] public int VariantColorId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Chọn dung lượng trong danh mục biến thể")] public int VariantStorageId { get; set; }
    [Range(0, 999999999)] public decimal Price { get; set; }
    [Range(0, 999999999)] public decimal? OldPrice { get; set; }
    [Range(0, 100000)] public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class VariantOptionsViewModel
{
    public List<VariantColor> Colors { get; set; } = [];
    public List<VariantStorage> Storages { get; set; } = [];
}
