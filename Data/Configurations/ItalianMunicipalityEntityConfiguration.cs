using DashboardOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardOrders.Data.Configurations;

public sealed class ItalianMunicipalityEntityConfiguration : IEntityTypeConfiguration<ItalianMunicipalityEntity>
{
    public void Configure(EntityTypeBuilder<ItalianMunicipalityEntity> builder)
    {
        builder.ToTable("ItalianMunicipalities");
        builder.HasKey(municipality => municipality.Code);
        builder.HasIndex(municipality => municipality.Name);
        builder.HasIndex(municipality => municipality.ProvinceCode);
        builder.HasIndex(municipality => new { municipality.ProvinceCode, municipality.Name });
        builder.Property(municipality => municipality.Code).HasMaxLength(6).IsRequired();
        builder.Property(municipality => municipality.ProvinceCode).HasMaxLength(3).IsRequired();
        builder.Property(municipality => municipality.RegionCode).HasMaxLength(2).IsRequired();
        builder.Property(municipality => municipality.Name).HasMaxLength(100).IsRequired();
        builder.Property(municipality => municipality.CadastralCode).HasMaxLength(4);
        builder.Property(municipality => municipality.IsProvinceCapital).IsRequired();

        builder
            .HasOne(municipality => municipality.Province)
            .WithMany(province => province.Municipalities)
            .HasForeignKey(municipality => municipality.ProvinceCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(municipality => municipality.Region)
            .WithMany(region => region.Municipalities)
            .HasForeignKey(municipality => municipality.RegionCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
