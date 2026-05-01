using System.ComponentModel.DataAnnotations;

namespace DashboardOrders.Models.ViewModels;

public class NewOrderViewModel
{
    public string CategoryCode { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public int? Quantity { get; set; }

    public List<Category> Categories { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<NewOrderItemViewModel> Items { get; set; } = new();
}

public class NewOrderItemViewModel
{
    [Required(ErrorMessage = "Seleziona un prodotto.")]
    public string ProductCode { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La quantita deve essere almeno 1.")]
    public int Quantity { get; set; }
}
