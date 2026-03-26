namespace DashboardOrders.Models;

public class CustomersPageViewModel
{
    public List<CustomerOrdersSummaryViewModel> Customers { get; set; } = new();
    public int TotalCustomers { get; set; }
    public int CustomersWithOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string SearchTerm { get; set; } = string.Empty;
    public string SortBy { get; set; } = "totalAmount";
    public string SortDirection { get; set; } = "desc";
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool ShowAll => PageSize == 0;
    public bool HasPreviousPage => !ShowAll && CurrentPage > 1;
    public bool HasNextPage => !ShowAll && CurrentPage < TotalPages;
    public int PreviousPage => HasPreviousPage ? CurrentPage - 1 : 1;
    public int NextPage => HasNextPage ? CurrentPage + 1 : TotalPages;
    public bool HasActiveSearch => !string.IsNullOrWhiteSpace(SearchTerm);
}
