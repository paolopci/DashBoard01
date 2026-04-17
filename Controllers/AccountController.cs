using DashboardOrders.Models;
using DashboardOrders.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardOrders.Controllers;

public class AccountController : Controller
{
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
        return View(new Register());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Register? model)
    {
        model ??= new Register();
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
            return View(model);
        }

        var result = await accountService.LoginAsync(ToLoginDto(model));
        if (!result.Succeeded)
        {
            AddOperationErrorsToModelState(result);
            SetErrorToast(result.Message);
            return View(model);
        }

        SetSuccessToast(result.Message);
        return RedirectToLocal(returnUrl);
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
            ModelState.AddModelError(nameof(Models.Register.DateOfBirth), "La data di nascita è obbligatoria.");
        }
        else if (model.DateOfBirth.Date >= DateTime.Today)
        {
            ModelState.AddModelError(nameof(Models.Register.DateOfBirth), "La data di nascita deve essere nel passato.");
        }
    }

    private void ValidateLogin(Login model)
    {
        if (string.IsNullOrWhiteSpace(model.UserLogin))
        {
            ModelState.AddModelError(nameof(Models.Login.UserLogin), "Il login è obbligatorio.");
        }

        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(Models.Login.Password), "La password è obbligatoria.");
        }
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
