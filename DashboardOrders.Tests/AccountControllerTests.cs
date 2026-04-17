using DashboardOrders.Controllers;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using Xunit;

namespace DashboardOrders.Tests;

public class AccountControllerTests
{
    private readonly IAccountService accountService;
    private readonly AccountController sut;

    public AccountControllerTests()
    {
        accountService = Substitute.For<IAccountService>();
        sut = new AccountController(accountService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public void Register_Get_QuandoRichiesto_AlloraRestituisceVista()
    {
        // Arrange

        // Act
        var risultato = sut.Register();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<Register>();
    }

    [Fact]
    public async Task Register_Post_QuandoModelValido_AlloraReindirizzaALogin()
    {
        // Arrange
        var model = CreateRegisterModel();
        accountService.RegisterAsync(Arg.Any<RegisterDto>())
            .Returns(AccountOperationResult.Success("Registrazione completata."));

        // Act
        var risultato = await sut.Register(model);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(AccountController.Login));
    }

    [Fact]
    public async Task Register_Post_QuandoModelNull_AlloraRestituisceVistaConErrore()
    {
        // Arrange
        Register? model = null;

        // Act
        var risultato = await sut.Register(model);

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Register_Post_QuandoServizioFallisce_AlloraRestituisceVistaConModelStateNonValido()
    {
        // Arrange
        var model = CreateRegisterModel();
        accountService.RegisterAsync(Arg.Any<RegisterDto>())
            .Returns(AccountOperationResult.Failure("Registrazione non completata.", ["Utente già presente."]));

        // Act
        var risultato = await sut.Register(model);

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Login_Get_QuandoRichiesto_AlloraRestituisceVista()
    {
        // Arrange

        // Act
        var risultato = sut.Login();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeOfType<Login>();
    }

    [Fact]
    public async Task Login_Post_QuandoCredenzialiValide_AlloraReindirizzaAIndex()
    {
        // Arrange
        var model = new Login { UserLogin = "mario.rossi@example.com", Password = "Password1" };
        accountService.LoginAsync(Arg.Any<LoginDto>())
            .Returns(AccountOperationResult.Success("Accesso eseguito.", token: "token"));

        // Act
        var risultato = await sut.Login(model);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Login_Post_QuandoModelNull_AlloraRestituisceVistaConErrore()
    {
        // Arrange
        Login? model = null;

        // Act
        var risultato = await sut.Login(model);

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Login_Post_QuandoServizioFallisce_AlloraRestituisceVistaConModelStateNonValido()
    {
        // Arrange
        var model = new Login { UserLogin = "mario.rossi@example.com", Password = "Password1" };
        accountService.LoginAsync(Arg.Any<LoginDto>())
            .Returns(AccountOperationResult.Failure("Email o password non validi."));

        // Act
        var risultato = await sut.Login(model);

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Logout_Post_QuandoServizioRiesce_AlloraReindirizzaAIndex()
    {
        // Arrange
        accountService.LogoutAsync(Arg.Any<LogoutDto>())
            .Returns(AccountOperationResult.Success("Logout eseguito."));

        // Act
        var risultato = await sut.Logout(new Logout());

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
    }

    [Fact]
    public void AccessDenied_QuandoRichiesto_AlloraRestituisceVistaConStatus403()
    {
        // Arrange

        // Act
        var risultato = sut.AccessDenied();

        // Assert
        risultato.Should().BeOfType<ViewResult>();
        sut.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    private static Register CreateRegisterModel()
    {
        return new Register
        {
            FirstName = "Mario",
            LastName = "Rossi",
            Email = "mario.rossi@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            City = "Milano",
            Country = "Italia",
            Cap = "RSSMRA90A01F205X",
            Password = "Password1",
            RepeatPassword = "Password1"
        };
    }
}
