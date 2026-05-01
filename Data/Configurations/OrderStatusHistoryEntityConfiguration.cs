using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class OrderStatusHistoryEntityConfiguration : IEntityTypeConfiguration<OrderStatusHistoryEntity>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistoryEntity> builder)
    {
        builder.ToTable("OrderStatusHistory");
        builder.HasKey(history => history.Id);
        builder.HasIndex(history => history.OrderId);
        builder.HasIndex(history => history.ChangedAt);
        builder.Property(history => history.ChangedAt).IsRequired();
        builder.Property(history => history.ChangedBy).HasMaxLength(256);
        builder.Property(history => history.Reason).HasMaxLength(500);
        builder.Property(history => history.CorrelationId).HasMaxLength(100);

        builder
            .HasOne(history => history.Order)
            .WithMany(order => order.StatusHistory)
            .HasForeignKey(history => history.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
