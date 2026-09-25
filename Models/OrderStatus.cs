namespace ShopMobileWorld.Models;

public static class OrderStatus
{
    public const string New = "Mới đặt";
    public const string Processing = "Đang xử lý";
    public const string Shipping = "Đang giao";
    public const string Completed = "Hoàn thành";
    public const string Cancelled = "Đã hủy";

    public static IReadOnlyList<string> AvailableFrom(string current) => current switch
    {
        New => [New, Processing, Shipping, Completed, Cancelled],
        Processing => [Processing, Shipping, Completed, Cancelled],
        Shipping => [Shipping, Completed, Cancelled],
        Completed => [Completed],
        Cancelled => [Cancelled],
        _ => []
    };

    public static bool CanTransition(string current, string next) => AvailableFrom(current).Contains(next);
}
