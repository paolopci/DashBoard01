namespace DashboardOrders.Models.ViewModels;

public class EditCustomerViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarInitials { get; set; } = string.Empty;
    public string PhonePrefix { get; set; } = string.Empty;
    public string PhoneCountryIso2 { get; set; } = string.Empty;
    public string PhoneCountryFlagPath { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<PhoneCountryPrefixViewModel> PhoneCountryPrefixes { get; set; } = [];
}
