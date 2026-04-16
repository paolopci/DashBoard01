namespace DashboardOrders.Models;

public sealed record OrderStatusPresentation(string RowAccent, string BackgroundColor, string ForegroundColor, string Label)
{
    public static OrderStatusPresentation FromStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Delivered => new("bg-emerald-500", "#d1fae5", "#065f46", "Consegnato"),
            OrderStatus.Shipped => new("bg-blue-500", "#dbeafe", "#1e40af", "Spedito"),
            OrderStatus.Processing => new("bg-indigo-500", "#e0e7ff", "#3730a3", "Elaborazione"),
            OrderStatus.Pending => new("bg-amber-500", "#fef3c7", "#92400e", "In Attesa"),
            OrderStatus.Cancelled => new("bg-rose-500", "#fee2e2", "#991b1b", "Annullato"),
            _ => new("bg-slate-400", "#f1f5f9", "#475569", "Sconosciuto")
        };
    }
}
