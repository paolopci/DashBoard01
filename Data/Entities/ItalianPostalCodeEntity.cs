namespace DashboardOrders.Data.Entities;

public class ItalianPostalCodeEntity
{
    public int Id { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public string ProvinceCode { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}
