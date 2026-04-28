namespace DashboardOrders.Services;

public sealed class ItalianTerritoryImportOptions
{
    public required string IstatMunicipalitiesXlsxPath { get; init; }
    public string? PostalCodesCsvPath { get; init; }
    public int ExpectedRegionCount { get; init; } = 20;
    public int ExpectedMunicipalityCount { get; init; } = 7894;
}
