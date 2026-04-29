using DashboardOrders.Models.Dto;
using DashboardOrders.Models.ViewModels;
using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

public class AccountController : Controller
{
    private const string AdminEmail = "admin@micene.it";
    private const string ToastSuccessKey = "Toast.Success";
    private const string ToastErrorKey = "Toast.Error";
    private const string GenericRegisterErrorMessage = "Controlla i dati inseriti e riprova.";
    private const string GenericLoginErrorMessage = "Email o password non validi.";

    private readonly IAccountService accountService;

    public AccountController(IAccountService accountService)
    {
        this.accountService = accountService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(CreateRegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Register? model)
    {
        if (model is null)
        {
            model = CreateRegisterViewModel();
            ModelState.AddModelError(string.Empty, GenericRegisterErrorMessage);
        }

        ValidateRegister(model);

        if (!ModelState.IsValid)
        {
            SetErrorToast(GenericRegisterErrorMessage);
            return View(model);
        }

        var result = await accountService.RegisterAsync(ToRegisterDto(model));
        if (!result.Succeeded)
        {
            AddOperationErrorsToModelState(result);
            SetErrorToast(result.Message);
            return View(model);
        }

        SetSuccessToast(result.Message);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new Login());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(Login? model, string? returnUrl = null)
    {
        model ??= new Login();
        ViewData["ReturnUrl"] = returnUrl;
        ValidateLogin(model);

        if (!ModelState.IsValid)
        {
            SetErrorToast(GenericLoginErrorMessage);
            ClearLoginFields(model);
            ValidateLogin(model);
            return View(model);
        }

        var result = await accountService.LoginAsync(ToLoginDto(model));
        if (!result.Succeeded)
        {
            AddOperationErrorsToModelState(result);
            SetErrorToast(result.Message);
            ClearLoginFields(model);
            return View(model);
        }

        SetSuccessToast(result.Message);
        return RedirectAfterLogin(returnUrl, result.Email ?? model.UserLogin);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Logout(string? returnUrl = null)
    {
        return View(new Logout { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(Logout? model)
    {
        var result = await accountService.LogoutAsync(new LogoutDto(model?.ReturnUrl));
        if (!result.Succeeded)
        {
            SetErrorToast(result.Message);
            return View(model ?? new Logout());
        }

        SetSuccessToast(result.Message);
        return RedirectToLocal(model?.ReturnUrl);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var profile = await accountService.GetProfileAsync(User.Identity?.Name);
        if (profile is null)
        {
            return RedirectToAction(nameof(AccessDenied));
        }

        return View(profile);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var profile = await accountService.GetProfileAsync(User.Identity?.Name);
        if (profile is null)
        {
            return RedirectToAction(nameof(AccessDenied));
        }

        return View(profile);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(UserProfileViewModel? model)
    {
        model ??= new UserProfileViewModel();

        var currentProfile = await accountService.GetProfileAsync(User.Identity?.Name);
        if (currentProfile is null)
        {
            return RedirectToAction(nameof(AccessDenied));
        }

        model.FirstName = currentProfile.FirstName;
        model.LastName = currentProfile.LastName;
        model.Email = currentProfile.Email;

        if (!ModelState.IsValid)
        {
            SetErrorToast("Controlla i dati del profilo e riprova.");
            return View(model);
        }

        var result = await accountService.UpdateProfileAsync(User.Identity?.Name, model);
        if (!result.Succeeded)
        {
            AddOperationErrorsToModelState(result);
            SetErrorToast(result.Message);
            return View(model);
        }

        SetSuccessToast(result.Message);
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private void ValidateRegister(Register model)
    {
        if (model.DateOfBirth == default)
        {
            ModelState.AddModelError(nameof(DashboardOrders.Models.ViewModels.Register.DateOfBirth), "La data di nascita è obbligatoria.");
        }
        else if (model.DateOfBirth.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(DashboardOrders.Models.ViewModels.Register.DateOfBirth), "La data di nascita non può essere nel futuro.");
        }
    }

    private static Register CreateRegisterViewModel()
    {
        return new Register { DateOfBirth = DateTime.Today };
    }

    private void ValidateLogin(Login model)
    {
        if (string.IsNullOrWhiteSpace(model.UserLogin))
        {
            ModelState.AddModelError(nameof(DashboardOrders.Models.ViewModels.Login.UserLogin), "Il login è obbligatorio.");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(DashboardOrders.Models.ViewModels.Login.Password), "La password è obbligatoria.");
        }
    }

    private void ClearLoginFields(Login model)
    {
        model.UserLogin = string.Empty;
        model.Password = string.Empty;
        ModelState.Remove(nameof(DashboardOrders.Models.ViewModels.Login.UserLogin));
        ModelState.Remove(nameof(DashboardOrders.Models.ViewModels.Login.Password));
    }

    private static RegisterDto ToRegisterDto(Register model)
    {
        return new RegisterDto(
            model.FirstName,
            model.LastName,
            model.Email,
            model.DateOfBirth,
            model.City,
            model.Country,
            model.Cap,
            model.Password,
            model.RepeatPassword);
    }

    private static LoginDto ToLoginDto(Login model)
    {
        return new LoginDto(model.UserLogin, model.Password);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectAfterLogin(string? returnUrl, string? email)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return IsAdmin(email)
            ? RedirectToAction("Index", "Home")
            : RedirectToAction("Orders", "Home");
    }

    private static bool IsAdmin(string? email)
    {
        return string.Equals(email?.Trim(), AdminEmail, StringComparison.OrdinalIgnoreCase);
    }

    private void AddOperationErrorsToModelState(AccountOperationResult result)
    {
        var errors = result.Errors ?? [];
        if (errors.Count == 0)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return;
        }

        foreach (var error in errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }
    }

    private void SetSuccessToast(string message)
    {
        TempData[ToastSuccessKey] = message;
    }

    private void SetErrorToast(string message)
    {
        TempData[ToastErrorKey] = message;
    }
}
