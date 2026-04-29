using DashboardOrders.Models.ViewModels;
using DashboardOrders.Data;
using DashboardOrders.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardOrders.Services;

public class DashboardAnalyticsService(DashboardOrdersDbContext dbContext) : IDashboardAnalyticsService
{
    public DashboardAnalyticsViewModel GetAnalytics(string period = "30d")
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var totalOrders = dbContext.Orders.Count();
        var totalRevenue = dbContext.Orders
            .AsEnumerable()
            .Where(order => OrderStatusMetricsPolicy.IsRevenueRelevant((OrderStatus)order.Status))
            .Sum(o => o.TotalAmount);
        var lowStockProducts = dbContext.Products.Count(p => p.StockQuantity < 10);
        var activeCustomers = dbContext.Customers.Count(c => c.Orders.Any());

        var orderTrend = dbContext.Orders
            .Where(o => o.CreatedAt >= thirtyDaysAgo)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new TrendData(g.Key.ToString("yyyy-MM-dd"), g.Count()))
            .OrderBy(t => t.Label)
            .ToList();

        var metrics = new List<Metric>
        {
            new("Ordini Totali", totalOrders.ToString(), "+12%", "📦", "indigo"),
            new("Ricavo", totalRevenue.ToString("C"), "+8%", "💰", "emerald"),
            new("Prodotti Basso Stock", lowStockProducts.ToString(), "-3%", "⚠️", "amber"),
            new("Clienti Attivi", activeCustomers.ToString(), "+5%", "👥", "blue")
        };

        return new DashboardAnalyticsViewModel
        {
            Metrics = metrics,
            OrderTrend = orderTrend,
            Period = period
        };
    }
}
