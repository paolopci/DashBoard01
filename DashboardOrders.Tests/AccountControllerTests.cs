using DashboardOrders.Controllers;
using DashboardOrders.Models;
using DashboardOrders.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Security.Claims;
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
    public void Register_Get_QuandoRichiesto_AlloraInizializzaDataDiNascitaAOggi()
    {
        // Arrange
        var dataAttesa = DateTime.Today;

        // Act
        var risultato = sut.Register();

        // Assert
        risultato.Should().BeOfType<ViewResult>()
            .Which.Model.Should().BeOfType<Register>()
            .Which.DateOfBirth.Date.Should().Be(dataAttesa);
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
    public async Task Register_Post_QuandoDataDiNascitaFutura_AlloraRestituisceVistaConModelStateNonValido()
    {
        // Arrange
        var model = CreateRegisterModel();
        model.DateOfBirth = DateTime.Today.AddDays(1);

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
    public async Task Login_Post_QuandoCredenzialiAdminValide_AlloraReindirizzaAIndex()
    {
        // Arrange
        var model = new Login { UserLogin = "admin@micene.it", Password = "Password1" };
        accountService.LoginAsync(Arg.Any<LoginDto>())
            .Returns(AccountOperationResult.Success("Accesso eseguito.", email: "admin@micene.it", token: "token"));

        // Act
        var risultato = await sut.Login(model);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Login_Post_QuandoCredenzialiUtenteValide_AlloraReindirizzaAOrders()
    {
        // Arrange
        var model = new Login { UserLogin = "giulia.lombardi65@example.com", Password = "Password1" };
        accountService.LoginAsync(Arg.Any<LoginDto>())
            .Returns(AccountOperationResult.Success("Accesso eseguito.", email: "giulia.lombardi65@example.com", token: "token"));

        // Act
        var risultato = await sut.Login(model);

        // Assert
        var redirect = risultato.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Orders");
        redirect.ControllerName.Should().Be("Home");
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
    public async Task Profile_QuandoProfiloEsiste_AlloraRestituisceVistaConProfilo()
    {
        // Arrange
        var profile = CreateProfile();
        sut.ControllerContext.HttpContext.User = CreateUser(profile.Email);
        accountService.GetProfileAsync(profile.Email).Returns(profile);

        // Act
        var risultato = await sut.Profile();

        // Assert
        risultato.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(profile);
    }

    [Fact]
    public async Task EditProfile_Post_QuandoModelValido_AlloraNonModificaNomeCognomeEmail()
    {
        // Arrange
        var currentProfile = CreateProfile();
        sut.ControllerContext.HttpContext.User = CreateUser(currentProfile.Email);
        accountService.GetProfileAsync(currentProfile.Email).Returns(currentProfile);
        accountService.UpdateProfileAsync(currentProfile.Email, Arg.Any<UserProfileViewModel>())
            .Returns(AccountOperationResult.Success("Profilo aggiornato."));
        var model = new UserProfileViewModel
        {
            FirstName = "Alterato",
            LastName = "Alterato",
            Email = "altro@example.com",
            DateOfBirth = new DateTime(1991, 2, 3),
            City = "Roma",
            Country = "Italia",
            FiscalCode = "RSSMRA91B03H501Y"
        };

        // Act
        var risultato = await sut.EditProfile(model);

        // Assert
        risultato.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(AccountController.Profile));
        await accountService.Received(1).UpdateProfileAsync(
            currentProfile.Email,
            Arg.Is<UserProfileViewModel>(profile =>
                profile.FirstName == currentProfile.FirstName &&
                profile.LastName == currentProfile.LastName &&
                profile.Email == currentProfile.Email &&
                profile.City == "Roma"));
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

    private static UserProfileViewModel CreateProfile()
    {
        return new UserProfileViewModel
        {
            FirstName = "Mario",
            LastName = "Rossi",
            Email = "mario.rossi@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            City = "Milano",
            Country = "Italia",
            FiscalCode = "RSSMRA90A01F205X"
        };
    }

    private static ClaimsPrincipal CreateUser(string email)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, email)],
            authenticationType: "Test"));
    }
}
