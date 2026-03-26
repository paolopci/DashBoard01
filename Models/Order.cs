namespace DashboardOrders.Models;

public class Order
{
    private List<OrderItem> _items = new();

    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Customer Customer { get; set; } = new();
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items
    {
        get => _items;
        set => _items = value.Count > 7 ? value.Take(7).ToList() : value;
    }

    public string Product => Items.FirstOrDefault()?.ProductName ?? string.Empty;
    public int Quantity => Items.Sum(item => item.Quantity);
    public int ItemsCount => Items.Count;
}
