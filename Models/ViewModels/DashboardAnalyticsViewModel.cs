using System.Collections.Generic;

namespace DashboardOrders.Models.ViewModels;

public class DashboardAnalyticsViewModel
{
    public List<Metric> Metrics { get; set; } = new();
    public List<TrendData> OrderTrend { get; set; } = new();
    public string Period { get; set; } = "30d";
}