using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DashboardOrders.Tests;

public class ItalianAdministrativeTerritoryMappingTests
{
    [Fact]
    public void Model_QuandoConfigurato_AlloraUsaTabelleTerritorialiNormalizzate()
    {
        // Arrange
        using var dbContext = CreateDbContext();

        // Act
        var region = dbContext.Model.FindEntityType(typeof(ItalianRegionEntity));
        var province = dbContext.Model.FindEntityType(typeof(ItalianProvinceEntity));
        var municipality = dbContext.Model.FindEntityType(typeof(ItalianMunicipalityEntity));

        // Assert
        Assert.NotNull(region);
        Assert.NotNull(province);
        Assert.NotNull(municipality);
        region.GetTableName().Should().Be("ItalianRegions");
        province.GetTableName().Should().Be("ItalianProvinces");
        municipality.GetTableName().Should().Be("ItalianMunicipalities");

        var regionPrimaryKey = region.FindPrimaryKey();
        var provincePrimaryKey = province.FindPrimaryKey();
        var municipalityPrimaryKey = municipality.FindPrimaryKey();

        Assert.NotNull(regionPrimaryKey);
        Assert.NotNull(provincePrimaryKey);
        Assert.NotNull(municipalityPrimaryKey);
        regionPrimaryKey.Properties.Should().ContainSingle(property => property.Name == nameof(ItalianRegionEntity.Code));
        provincePrimaryKey.Properties.Should().ContainSingle(property => property.Name == nameof(ItalianProvinceEntity.Code));
        municipalityPrimaryKey.Properties.Should().ContainSingle(property => property.Name == nameof(ItalianMunicipalityEntity.Code));
    }

    [Fact]
    public async Task ItalianAdministrativeTerritories_QuandoPersistite_AlloraMantengonoRelazioni()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        dbContext.ItalianRegions.Add(new ItalianRegionEntity
        {
            Code = "11",
            Name = "Marche",
            Nuts1Code = "ITI",
            Nuts2Code = "ITI3"
        });
        dbContext.ItalianProvinces.Add(new ItalianProvinceEntity
        {
            Code = "041",
            RegionCode = "11",
            Name = "Pesaro e Urbino",
            Abbreviation = "PU",
            Nuts3Code = "ITI31"
        });
        dbContext.ItalianMunicipalities.Add(new ItalianMunicipalityEntity
        {
            Code = "041044",
            ProvinceCode = "041",
            RegionCode = "11",
            Name = "Pesaro",
            CadastralCode = "G479",
            IsProvinceCapital = true
        });

        // Act
        await dbContext.SaveChangesAsync();
        var municipality = await dbContext.ItalianMunicipalities
            .Include(existing => existing.Province)
            .ThenInclude(existing => existing.Region)
            .SingleAsync(existing => existing.Code == "041044");

        // Assert
        municipality.Name.Should().Be("Pesaro");
        municipality.Province.Name.Should().Be("Pesaro e Urbino");
        municipality.Province.Region.Name.Should().Be("Marche");
    }

    private static DashboardOrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DashboardOrdersDbContext(options);
    }
}
