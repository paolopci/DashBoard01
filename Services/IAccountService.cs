using DashboardOrders.Models.Dto;
using DashboardOrders.Models.ViewModels;
using DashboardOrders.Models;

namespace DashboardOrders.Services;

public interface IAccountService
{
    Task<AccountOperationResult> RegisterAsync(RegisterDto? registerDto);

    Task<AccountOperationResult> LoginAsync(LoginDto? loginDto);

    Task<AccountOperationResult> LogoutAsync(LogoutDto? logoutDto);

    Task<UserProfileViewModel?> GetProfileAsync(string? email);

    Task<AccountOperationResult> UpdateProfileAsync(string? email, UserProfileViewModel? profile);
}
