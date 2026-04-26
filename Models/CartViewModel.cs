namespace DashboardOrders.Models;

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int TotalItems => Items.Sum(item => item.Quantity);
    public decimal TotalAmount => Items.Sum(item => item.LineTotal);
    public bool HasUnavailableItems => Items.Any(item => !item.IsAvailable);
}

public class CartItemViewModel
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public decimal LineTotal => UnitPrice * Quantity;
    public bool IsAvailable => Stock > 0 && Quantity <= Stock;
}
