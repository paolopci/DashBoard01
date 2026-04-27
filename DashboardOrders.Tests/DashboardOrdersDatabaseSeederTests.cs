using DashboardOrders.Data;
using DashboardOrders.Data.Entities;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DashboardOrders.Tests;

public class DashboardOrdersDatabaseSeederTests
{
    [Fact]
    public async Task SeedAsync_QuandoEseguito_AlloraCreaCinqueImmaginiCarouselPerProdotto()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = new DashboardOrdersDatabaseSeeder(dbContext, CreateUserManager(), CreateRoleManager());

        // Act
        await sut.SeedAsync();

        // Assert
        var productCount = await dbContext.Products.CountAsync();
        var imagesByProduct = await dbContext.ProductCarouselImages
            .GroupBy(image => image.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Count = group.Count(),
                DisplayOrders = group.Select(image => image.DisplayOrder).OrderBy(displayOrder => displayOrder).ToList()
            })
            .ToListAsync();
        var expectedDisplayOrders = new[] { 1, 2, 3, 4, 5 };

        imagesByProduct.Should().HaveCount(productCount);
        imagesByProduct.Should().OnlyContain(group =>
            group.Count == 5 &&
            group.DisplayOrders.SequenceEqual(expectedDisplayOrders));
    }

    [Fact]
    public async Task SeedAsync_QuandoCreaImmaginiCarousel_AlloraNonRiusaUrl()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var sut = new DashboardOrdersDatabaseSeeder(dbContext, CreateUserManager(), CreateRoleManager());

        // Act
        await sut.SeedAsync();

        // Assert
        var imageUrls = await dbContext.ProductCarouselImages
            .Select(image => image.ImageUrl)
            .ToListAsync();
        imageUrls.Should().OnlyHaveUniqueItems();
    }

    private static DashboardOrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DashboardOrdersDbContext(options);
    }

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserPasswordStore<ApplicationUser>>();
        var userManager = Substitute.For<UserManager<ApplicationUser>>(
            store,
            Options.Create(new IdentityOptions()),
            Substitute.For<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Substitute.For<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<ApplicationUser>>>());

        userManager.FindByEmailAsync(Arg.Any<string>()).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.Users.Returns(Array.Empty<ApplicationUser>().AsQueryable());

        return userManager;
    }

    private static RoleManager<IdentityRole> CreateRoleManager()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        var roleManager = Substitute.For<RoleManager<IdentityRole>>(
            store,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Substitute.For<ILogger<RoleManager<IdentityRole>>>());

        roleManager.FindByNameAsync(Arg.Any<string>()).Returns(call => new IdentityRole(call.Arg<string>()));

        return roleManager;
    }
}
