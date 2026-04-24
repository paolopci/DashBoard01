namespace DashboardOrders.Models;

public sealed record OrderStatusPresentation(string RowAccent, string BackgroundColor, string ForegroundColor, string Label)
{
    public static OrderStatusPresentation FromStatus(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Cart => new("bg-slate-400", "#e2e8f0", "#334155", "Carrello"),
            OrderStatus.Pending => new("bg-amber-500", "#fef3c7", "#92400e", "In Attesa"),
            OrderStatus.PaymentPending => new("bg-amber-500", "#fde68a", "#92400e", "Pagamento In Attesa"),
            OrderStatus.PaymentAuthorized => new("bg-cyan-500", "#cffafe", "#155e75", "Pagamento Autorizzato"),
            OrderStatus.PaymentFailed => new("bg-rose-500", "#fee2e2", "#991b1b", "Pagamento Fallito"),
            OrderStatus.Confirmed => new("bg-teal-500", "#ccfbf1", "#115e59", "Confermato"),
            OrderStatus.Delivered => new("bg-emerald-500", "#d1fae5", "#065f46", "Consegnato"),
            OrderStatus.Processing => new("bg-indigo-500", "#e0e7ff", "#3730a3", "Elaborazione"),
            OrderStatus.Picking => new("bg-indigo-500", "#e0e7ff", "#3730a3", "Picking"),
            OrderStatus.Packing => new("bg-violet-500", "#ede9fe", "#5b21b6", "Packing"),
            OrderStatus.Shipped => new("bg-blue-500", "#dbeafe", "#1e40af", "Spedito"),
            OrderStatus.Cancelled => new("bg-rose-500", "#fee2e2", "#991b1b", "Annullato"),
            OrderStatus.ReturnRequested => new("bg-orange-500", "#ffedd5", "#9a3412", "Reso Richiesto"),
            OrderStatus.ReturnApproved => new("bg-orange-500", "#fed7aa", "#9a3412", "Reso Approvato"),
            OrderStatus.Returned => new("bg-orange-500", "#fdba74", "#9a3412", "Reso Ricevuto"),
            OrderStatus.Refunded => new("bg-slate-500", "#e2e8f0", "#334155", "Rimborsato"),
            OrderStatus.PartiallyRefunded => new("bg-emerald-500", "#dcfce7", "#166534", "Parzialmente Rimborsato"),
            _ => new("bg-slate-400", "#f1f5f9", "#475569", "Sconosciuto")
        };
    }
}
