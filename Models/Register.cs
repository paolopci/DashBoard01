using System.ComponentModel.DataAnnotations;

namespace DashboardOrders.Models;

public class Register
{
    [Required(ErrorMessage = "Il nome è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il nome non può superare 100 caratteri.")]
    [Display(Name = "Nome")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il cognome è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il cognome non può superare 100 caratteri.")]
    [Display(Name = "Cognome")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'email è obbligatoria.")]
    [EmailAddress(ErrorMessage = "Inserisci un indirizzo email valido.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La data di nascita è obbligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data di nascita")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "La città è obbligatoria.")]
    [StringLength(100, ErrorMessage = "La città non può superare 100 caratteri.")]
    [Display(Name = "Città")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il paese è obbligatorio.")]
    [StringLength(100, ErrorMessage = "Il paese non può superare 100 caratteri.")]
    [Display(Name = "Paese")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il codice fiscale è obbligatorio.")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "Il codice fiscale deve contenere 16 caratteri.")]
    [Display(Name = "Codice fiscale")]
    public string Cap { get; set; } = string.Empty;

    [Required(ErrorMessage = "La password è obbligatoria.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La password deve contenere almeno 8 caratteri.")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ripeti la password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Le password non coincidono.")]
    [Display(Name = "Ripeti password")]
    public string RepeatPassword { get; set; } = string.Empty;
}
