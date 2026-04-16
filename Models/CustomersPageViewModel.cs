namespace DashboardOrders.Models;

public class CustomersPageViewModel : PagedPageViewModel
{
    public List<CustomerOrdersSummaryViewModel> Customers { get; set; } = new();
    public int TotalCustomers { get; set; }
    public int CustomersWithOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public string SortBy { get; set; } = "totalAmount";
    public string SortDirection { get; set; } = "desc";
    public bool HasActiveSearch => !string.IsNullOrWhiteSpace(SearchTerm);
}
