using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class CustomerEntityConfiguration : IEntityTypeConfiguration<CustomerEntity>
{
    public void Configure(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.Name).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.Phone).HasMaxLength(20).IsRequired();
        builder.Property(customer => customer.Address).HasMaxLength(200);
        builder.Property(customer => customer.AvatarInitials).HasMaxLength(5).IsRequired();
    }
}
