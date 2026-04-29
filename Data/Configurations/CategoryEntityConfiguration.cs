using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class CategoryEntityConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(category => category.Code);
        builder.Property(category => category.Code).HasMaxLength(20);
        builder.Property(category => category.Name).HasMaxLength(100).IsRequired();
        builder.Property(category => category.Description).HasMaxLength(500).IsRequired();
    }
}
