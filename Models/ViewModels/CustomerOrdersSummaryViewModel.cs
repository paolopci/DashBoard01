namespace DashboardOrders.Models.ViewModels;

public class CustomerOrdersSummaryViewModel
{
    public Customer Customer { get; set; } = new();
    public int OrdersCount { get; set; }
    public decimal TotalOrdersAmount { get; set; }
}
