namespace DashboardOrders.Models.ViewModels;

public class DashboardViewModel : PagedPageViewModel
{
    public List<Order> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int ActiveCustomers { get; set; }
    public string SortBy { get; set; } = "date";
    public string SortDirection { get; set; } = "desc";
}
