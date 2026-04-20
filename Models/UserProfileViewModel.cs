using System.ComponentModel.DataAnnotations;

namespace DashboardOrders.Models;

public class UserProfileViewModel
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "La citta e obbligatoria.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il paese e obbligatorio.")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Il codice fiscale e obbligatorio.")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "Il codice fiscale deve contenere 16 caratteri.")]
    public string FiscalCode { get; set; } = string.Empty;
}
