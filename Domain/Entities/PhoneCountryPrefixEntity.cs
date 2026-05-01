namespace DashboardOrders.Domain.Entities;

public class PhoneCountryPrefixEntity
{
    public int Id { get; set; }
    public string Iso2 { get; set; } = string.Empty;
    public string Iso3 { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public string LocalizedCountryName { get; set; } = string.Empty;
    public string DialCode { get; set; } = string.Empty;
    public string FlagPath { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
