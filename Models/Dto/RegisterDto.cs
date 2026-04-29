namespace DashboardOrders.Models.Dto;

public sealed record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth,
    string City,
    string Country,
    string FiscalCode,
    string PhonePrefix,
    string PhoneCountryIso2,
    string PhoneNumber,
    string Password,
    string RepeatPassword);

