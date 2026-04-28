namespace DashboardOrders.Services;

public sealed class ItalianTerritoryImportException(string message) : InvalidOperationException(message);
