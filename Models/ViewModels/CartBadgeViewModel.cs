namespace DashboardOrders.Models.ViewModels;

public class CartBadgeViewModel
{
    public int ItemsCount { get; set; }
    public bool HasItems => ItemsCount > 0;
}
