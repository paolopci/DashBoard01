namespace DashboardOrders.Data.Entities;

public class ItalianProvinceEntity
{
    public string Code { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Abbreviation { get; set; }
    public string? Nuts3Code { get; set; }

    public ItalianRegionEntity Region { get; set; } = null!;
    public ICollection<ItalianMunicipalityEntity> Municipalities { get; set; } = [];
}
