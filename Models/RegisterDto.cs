namespace DashboardOrders.Models;

public sealed record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    DateTime DateOfBirth,
    string City,
    string Country,
    string FiscalCode,
    string Password,
    string RepeatPassword);
