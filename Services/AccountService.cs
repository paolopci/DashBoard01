using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DashboardOrders.Data.Entities;
using DashboardOrders.Extensions.Auth;
using DashboardOrders.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DashboardOrders.Services;

public class AccountService : IAccountService
{
    private const string GenericRegistrationError = "Registrazione non completata.";
    private const string GenericLoginError = "Email o password non validi.";

    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment environment;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
        this.environment = environment;
    }

    public async Task<AccountOperationResult> RegisterAsync(RegisterDto? registerDto)
    {
        if (registerDto is null)
        {
            return AccountOperationResult.Failure(GenericRegistrationError, ["Dati di registrazione mancanti."]);
        }

        if (!IsPasswordConfirmationValid(registerDto))
        {
            return AccountOperationResult.Failure(GenericRegistrationError, ["Le password non coincidono."]);
        }

        var normalizedEmail = NormalizeEmail(registerDto.Email);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return AccountOperationResult.Failure(GenericRegistrationError, ["L'email è obbligatoria."]);
        }

        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser is not null)
        {
            return AccountOperationResult.Failure(GenericRegistrationError, ["Un utente con questa email è già presente."]);
        }

        var applicationUser = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FirstName = registerDto.FirstName.Trim(),
            LastName = registerDto.LastName.Trim(),
            DateOfBirth = registerDto.DateOfBirth.Date,
            City = registerDto.City.Trim(),
            Country = registerDto.Country.Trim(),
            FiscalCode = registerDto.FiscalCode.Trim().ToUpperInvariant(),
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(applicationUser, registerDto.Password);
        if (!createResult.Succeeded)
        {
            return AccountOperationResult.Failure(
                GenericRegistrationError,
                createResult.Errors.Select(error => error.Description));
        }

        return AccountOperationResult.Success(
            "Registrazione completata. Ora puoi accedere.",
            applicationUser.Id,
            applicationUser.Email);
    }

    public async Task<AccountOperationResult> LoginAsync(LoginDto? loginDto)
    {
        if (loginDto is null)
        {
            return AccountOperationResult.Failure(GenericLoginError, ["Dati di login mancanti."]);
        }

        var normalizedEmail = NormalizeEmail(loginDto.Login);
        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return AccountOperationResult.Failure(GenericLoginError, ["Il login è obbligatorio."]);
        }

        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return AccountOperationResult.Failure(GenericLoginError);
        }

        var signInResult = await signInManager.PasswordSignInAsync(
            user,
            loginDto.Password,
            isPersistent: false,
            lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            return AccountOperationResult.Failure(GenericLoginError);
        }

        var jwtOptions = JwtOptions.Resolve(configuration, environment);
        var tokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);
        var token = GenerateJwtToken(user, jwtOptions, tokenExpiresAt);

        return AccountOperationResult.Success(
            "Accesso eseguito correttamente.",
            user.Id,
            user.Email,
            token,
            tokenExpiresAt);
    }

    public async Task<AccountOperationResult> LogoutAsync(LogoutDto? logoutDto)
    {
        await signInManager.SignOutAsync();

        return AccountOperationResult.Success(
            "Logout eseguito correttamente.",
            email: null);
    }

    private static bool IsPasswordConfirmationValid(RegisterDto registerDto)
    {
        return string.Equals(registerDto.Password, registerDto.RepeatPassword, StringComparison.Ordinal);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string GenerateJwtToken(ApplicationUser user, JwtOptions jwtOptions, DateTimeOffset tokenExpiresAt)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: tokenExpiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
