using Microsoft.AspNetCore.Identity;

namespace DashboardOrders.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string FiscalCode { get; set; } = string.Empty;

    public string PhonePrefix { get; set; } = string.Empty;

    public string PhoneCountryIso2 { get; set; } = string.Empty;
}

