using System.ComponentModel.DataAnnotations;

namespace DashboardOrders.Models;

public class NewOrderViewModel
{
    [Required(ErrorMessage = "Seleziona un prodotto.")]
    public string ProductCode { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "La quantita deve essere almeno 1.")]
    public int Quantity { get; set; } = 1;

    public List<Product> Products { get; set; } = new();
}
