namespace ShopMobileWorld.Models;

public class RevenueDay
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int NewOrders { get; set; }
    public int CompletedOrders { get; set; }
}

public class RevenueViewModel
{
    public int Days { get; set; }
    public List<RevenueDay> DaysData { get; set; } = [];
    public decimal TotalRevenue => DaysData.Sum(x => x.Revenue);
    public int NewOrders => DaysData.Sum(x => x.NewOrders);
    public int CompletedOrders => DaysData.Sum(x => x.CompletedOrders);
    public decimal PreviousRevenue { get; set; }
    public decimal RevenueChangePercent => PreviousRevenue == 0 ? 0 : Math.Round((TotalRevenue - PreviousRevenue) / PreviousRevenue * 100, 1);
}
