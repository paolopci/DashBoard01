using DashboardOrders.Models.ViewModels;
using DashboardOrders.Data;
using DashboardOrders.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace DashboardOrders.Extensions.Auth;

public static class IdentityExtensions
{
    public static IServiceCollection AddDashboardIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<DashboardOrdersDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
        });

        services.AddAuthorization();

        return services;
    }
}
