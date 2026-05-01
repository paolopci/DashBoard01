namespace DashboardOrders.Models.ViewModels;

public class CustomersPageViewModel : PagedPageViewModel
{
    public List<CustomerOrdersSummaryViewModel> Customers { get; set; } = new();
    public int TotalCustomers { get; set; }
    public int CustomersWithOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string SortBy { get; set; } = "totalAmount";
    public string SortDirection { get; set; } = "desc";
}
