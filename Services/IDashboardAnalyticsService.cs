using DashboardOrders.Models.ViewModels;
using DashboardOrders.Models;

namespace DashboardOrders.Services;

public interface IDashboardAnalyticsService
{
    DashboardAnalyticsViewModel GetAnalytics(string period = "30d");
}