namespace DashboardOrders.Domain.Entities;

public class ItalianRegionEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Nuts1Code { get; set; }
    public string? Nuts2Code { get; set; }

    public ICollection<ItalianProvinceEntity> Provinces { get; set; } = [];
    public ICollection<ItalianMunicipalityEntity> Municipalities { get; set; } = [];
}
