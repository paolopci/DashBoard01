namespace DashboardOrders.Services;

public sealed record ItalianTerritoryImportResult(
    int RegionsImported,
    int ProvincesImported,
    int MunicipalitiesImported,
    int PostalCodesImported,
    int PostalCodesSkipped);
