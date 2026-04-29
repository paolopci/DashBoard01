using DashboardOrders.Models.Dto;
using DashboardOrders.Models.ViewModels;
using DashboardOrders.Domain.Entities;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DashboardOrders.Tests;

public class AccountServiceTests
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment environment;
    private readonly AccountService sut;

    public AccountServiceTests()
    {
        userManager = CreateUserManager();
        roleManager = CreateRoleManager();
        signInManager = CreateSignInManager(userManager);
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "DashboardOrders.Tests",
                ["Jwt:Audience"] = "DashboardOrders.Tests",
                ["Jwt:ExpirationMinutes"] = "30",
                ["Jwt:SecretKey"] = "test-secret-key-for-dashboard-orders-auth-tests"
            })
            .Build();
        environment = Substitute.For<IWebHostEnvironment>();
        environment.EnvironmentName.Returns(Environments.Development);
        sut = new AccountService(userManager, roleManager, signInManager, configuration, environment);
    }

    [Fact]
    public async Task RegisterAsync_QuandoDtoValido_AlloraRestituisceSuccesso()
    {
        // Arrange
        var dto = CreateRegisterDto();
        userManager.FindByEmailAsync(dto.Email.ToLowerInvariant()).Returns((ApplicationUser?)null);
        userManager.CreateAsync(Arg.Any<ApplicationUser>(), dto.Password).Returns(IdentityResult.Success);
        roleManager.FindByNameAsync("User").Returns((IdentityRole?)new IdentityRole("User"));
        userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), "User").Returns(IdentityResult.Success);

        // Act
        var risultato = await sut.RegisterAsync(dto);

        // Assert
        risultato.Succeeded.Should().BeTrue();
        await userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
    }

    [Fact]
    public async Task RegisterAsync_QuandoDtoNull_AlloraRestituisceErrore()
    {
        // Arrange
        RegisterDto? dto = null;

        // Act
        var risultato = await sut.RegisterAsync(dto);

        // Assert
        risultato.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterAsync_QuandoUtenteEsiste_AlloraRestituisceErrore()
    {
        // Arrange
        var dto = CreateRegisterDto();
        userManager.FindByEmailAsync(dto.Email.ToLowerInvariant()).Returns(new ApplicationUser { Email = dto.Email });

        // Act
        var risultato = await sut.RegisterAsync(dto);

        // Assert
        risultato.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_QuandoCredenzialiValide_AlloraRestituisceToken()
    {
        // Arrange
        var dto = new LoginDto("mario.rossi@example.com", "Password1");
        var user = new ApplicationUser
        {
            Id = "user-1",
            UserName = dto.Login,
            Email = dto.Login
        };
        userManager.FindByEmailAsync(dto.Login).Returns(user);
        signInManager
            .PasswordSignInAsync(user, dto.Password, isPersistent: false, lockoutOnFailure: false)
            .Returns(SignInResult.Success);

        // Act
        var risultato = await sut.LoginAsync(dto);

        // Assert
        risultato.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task LoginAsync_QuandoDtoNull_AlloraRestituisceErrore()
    {
        // Arrange
        LoginDto? dto = null;

        // Act
        var risultato = await sut.LoginAsync(dto);

        // Assert
        risultato.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task LogoutAsync_QuandoInvocato_AlloraEsegueSignOut()
    {
        // Arrange
        var dto = new LogoutDto(null);

        // Act
        await sut.LogoutAsync(dto);

        // Assert
        await signInManager.Received(1).SignOutAsync();
    }

    private static RegisterDto CreateRegisterDto()
    {
        return new RegisterDto(
            "Mario",
            "Rossi",
            "mario.rossi@example.com",
            new DateTime(1990, 1, 1),
            "Milano",
            "Italia",
            "RSSMRA90A01F205X",
            "+39",
            "IT",
            "3331234567",
            "Password1",
            "Password1");
    }

    private static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserPasswordStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store,
            Options.Create(new IdentityOptions()),
            Substitute.For<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Substitute.For<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<ApplicationUser>>>());
    }

    private static RoleManager<IdentityRole> CreateRoleManager()
    {
        var store = Substitute.For<IRoleStore<IdentityRole>>();
        return Substitute.For<RoleManager<IdentityRole>>(
            store,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Substitute.For<ILogger<RoleManager<IdentityRole>>>());
    }

    private static SignInManager<ApplicationUser> CreateSignInManager(UserManager<ApplicationUser> userManager)
    {
        return Substitute.For<SignInManager<ApplicationUser>>(
            userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Options.Create(new IdentityOptions()),
            Substitute.For<ILogger<SignInManager<ApplicationUser>>>(),
            Substitute.For<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<ApplicationUser>>());
    }
}
