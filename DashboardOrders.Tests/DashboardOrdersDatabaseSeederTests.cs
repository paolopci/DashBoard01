using DashboardOrders.Data;
using DashboardOrders.Domain.Entities;
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

    [Fact]
    public async Task SeedAsync_QuandoAdminEsiste_AlloraConfermaEmailERiallineaPasswordERuolo()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var admin = new ApplicationUser
        {
            Id = "admin-1",
            UserName = "admin@micene.it",
            Email = "admin@micene.it",
            EmailConfirmed = false
        };
        var userManager = CreateUserManager(admin);
        var sut = new DashboardOrdersDatabaseSeeder(dbContext, userManager, CreateRoleManager());

        // Act
        await sut.SeedAsync();

        // Assert
        admin.EmailConfirmed.Should().BeTrue();
        await userManager.Received(1).UpdateAsync(admin);
        await userManager.Received(1).RemovePasswordAsync(admin);
        await userManager.Received(1).AddPasswordAsync(admin, "Micene@65");
        await userManager.Received(1).AddToRoleAsync(admin, "Admin");
    }

    private static DashboardOrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DashboardOrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DashboardOrdersDbContext(options);
    }

    private static UserManager<ApplicationUser> CreateUserManager(ApplicationUser? existingAdmin = null)
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

        userManager.FindByEmailAsync(Arg.Any<string>()).Returns(call =>
        {
            var email = call.Arg<string>();
            return string.Equals(email, "admin@micene.it", StringComparison.OrdinalIgnoreCase)
                ? existingAdmin
                : null;
        });
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.UpdateAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        userManager.HasPasswordAsync(Arg.Any<ApplicationUser>()).Returns(true);
        userManager.RemovePasswordAsync(Arg.Any<ApplicationUser>()).Returns(IdentityResult.Success);
        userManager.AddPasswordAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.RemoveFromRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
        userManager.Users.Returns((existingAdmin is null ? [] : new[] { existingAdmin }).AsQueryable());
        userManager.IsInRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>()).Returns(false);

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
