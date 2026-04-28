namespace DashboardOrders.Data.Entities;

public class ItalianMunicipalityEntity
{
    public string Code { get; set; } = string.Empty;
    public string ProvinceCode { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CadastralCode { get; set; }
    public bool IsProvinceCapital { get; set; }

    public ItalianProvinceEntity Province { get; set; } = null!;
    public ItalianRegionEntity Region { get; set; } = null!;
}
