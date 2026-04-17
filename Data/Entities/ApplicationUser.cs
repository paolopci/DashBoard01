using Microsoft.AspNetCore.Identity;

namespace DashboardOrders.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string FiscalCode { get; set; } = string.Empty;
}
