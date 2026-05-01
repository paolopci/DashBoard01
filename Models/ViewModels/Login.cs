using System.ComponentModel.DataAnnotations;

namespace DashboardOrders.Models.ViewModels;

public class Login
{
    [Required(ErrorMessage = "L'email è obbligatoria.")]
    [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido.")]
    [Display(Name = "Login")]
    public string UserLogin { get; set; } = string.Empty;

    [Required(ErrorMessage = "La password è obbligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}
