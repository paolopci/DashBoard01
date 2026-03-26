namespace DashboardOrders.Models;

public class DashboardViewModel
{
    public List<Order> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int ActiveCustomers { get; set; }
}
