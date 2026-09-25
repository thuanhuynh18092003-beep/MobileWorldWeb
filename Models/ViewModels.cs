using System.ComponentModel.DataAnnotations;

namespace ShopMobileWorld.Models;

public class CatalogViewModel
{
    public List<Product> Products { get; set; } = [];
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
    public int Quantity { get; set; }
    public decimal Total => Product.Price * Quantity;
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
