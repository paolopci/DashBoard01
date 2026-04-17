using DashboardOrders.Models;

namespace DashboardOrders.Services;

public interface IAccountService
{
    Task<AccountOperationResult> RegisterAsync(RegisterDto? registerDto);

    Task<AccountOperationResult> LoginAsync(LoginDto? loginDto);

    Task<AccountOperationResult> LogoutAsync(LogoutDto? logoutDto);
}
