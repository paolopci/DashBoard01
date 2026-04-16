namespace DashboardOrders.Models;

public class OrdersPageViewModel : PagedPageViewModel
{
    public List<Order> Orders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public string SortBy { get; set; } = "date";
    public string SortDirection { get; set; } = "desc";
    public int? SelectedCustomerId { get; set; }
    public string SelectedCustomerName { get; set; } = string.Empty;
    public bool HasCustomerFilter => SelectedCustomerId.HasValue;
}
