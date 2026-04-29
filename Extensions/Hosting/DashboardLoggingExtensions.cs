namespace DashboardOrders.Extensions.Hosting;

public static class DashboardLoggingExtensions
{
    public static ILoggingBuilder AddDashboardLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();

        return logging;
    }
}
