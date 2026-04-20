using System.ComponentModel.DataAnnotations;

namespace DashboardOrders.Models;

public class ProductFormViewModel
{
    [Required(ErrorMessage = "Il codice prodotto e obbligatorio.")]
    [StringLength(30, ErrorMessage = "Il codice prodotto non puo superare 30 caratteri.")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il nome prodotto e obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome prodotto non puo superare 100 caratteri.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoria e obbligatoria.")]
    public string CategoryCode { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descrizione non puo superare 500 caratteri.")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "Il costo unitario deve essere maggiore di zero.")]
    public decimal UnitCost { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Lo stock non puo essere negativo.")]
    public int Stock { get; set; }

    public List<Category> Categories { get; set; } = new();
}
