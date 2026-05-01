using System.Reflection;
using DashboardOrders.Domain.Entities;
using DashboardOrders.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true, reloadOnChange: true);
builder.Logging.AddDashboardLogging();

builder.Services.AddControllersWithViews();
builder.Services.AddDashboardApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

if (await app.TryRunDashboardCommandAsync(args))
{
    return;
}

await EnsureAdminUserAsync(app);

app.UseDashboardRequestPipeline();

app.Run();

static async Task EnsureAdminUserAsync(WebApplication app)
{
    const string adminEmail = "admin@micene.it";
    const string adminPassword = "Micene@65";

    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "Micene",
            DateOfBirth = new DateTime(1980, 1, 1),
            City = "Milano",
            Country = "Italia",
            FiscalCode = "ADMINADMIN01"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            LogIdentityErrors(app, "Creazione admin fallita", result);
            return;
        }
    }
    else
    {
        adminUser.UserName = adminEmail;
        adminUser.Email = adminEmail;
        adminUser.EmailConfirmed = true;
        adminUser.FirstName = string.IsNullOrWhiteSpace(adminUser.FirstName) ? "Admin" : adminUser.FirstName;
        adminUser.LastName = string.IsNullOrWhiteSpace(adminUser.LastName) ? "Micene" : adminUser.LastName;
        adminUser.DateOfBirth = adminUser.DateOfBirth == default ? new DateTime(1980, 1, 1) : adminUser.DateOfBirth;
        adminUser.City = string.IsNullOrWhiteSpace(adminUser.City) ? "Milano" : adminUser.City;
        adminUser.Country = string.IsNullOrWhiteSpace(adminUser.Country) ? "Italia" : adminUser.Country;
        adminUser.FiscalCode = string.IsNullOrWhiteSpace(adminUser.FiscalCode) ? "ADMINADMIN01" : adminUser.FiscalCode;

        var updateResult = await userManager.UpdateAsync(adminUser);
        if (!updateResult.Succeeded)
        {
            LogIdentityErrors(app, "Aggiornamento admin fallito", updateResult);
            return;
        }
    }

    if (await roleManager.FindByNameAsync("Admin") is null)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    if (await userManager.HasPasswordAsync(adminUser))
    {
        var removePasswordResult = await userManager.RemovePasswordAsync(adminUser);
        if (!removePasswordResult.Succeeded)
        {
            LogIdentityErrors(app, "Reset password admin fallito", removePasswordResult);
            return;
        }
    }

    var addPasswordResult = await userManager.AddPasswordAsync(adminUser, adminPassword);
    if (!addPasswordResult.Succeeded)
    {
        LogIdentityErrors(app, "Impostazione password admin fallita", addPasswordResult);
        return;
    }

    app.Logger.LogInformation("Utente admin verificato con successo.");
}

static void LogIdentityErrors(WebApplication app, string message, IdentityResult result)
{
    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
    app.Logger.LogError("{Message}: {Errors}", message, errors);
}
