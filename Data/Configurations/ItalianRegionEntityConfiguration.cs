using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class ItalianRegionEntityConfiguration : IEntityTypeConfiguration<ItalianRegionEntity>
{
    public void Configure(EntityTypeBuilder<ItalianRegionEntity> builder)
    {
        builder.ToTable("ItalianRegions");
        builder.HasKey(region => region.Code);
        builder.HasIndex(region => region.Name).IsUnique();
        builder.Property(region => region.Code).HasMaxLength(2).IsRequired();
        builder.Property(region => region.Name).HasMaxLength(100).IsRequired();
        builder.Property(region => region.Nuts1Code).HasMaxLength(5);
        builder.Property(region => region.Nuts2Code).HasMaxLength(5);
    }
}
