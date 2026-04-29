using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.City).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Country).HasMaxLength(100).IsRequired();
        builder.Property(user => user.FiscalCode).HasMaxLength(16).IsRequired();
        builder.HasIndex(user => user.FiscalCode).IsUnique();
    }
}
