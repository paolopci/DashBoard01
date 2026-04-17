namespace DashboardOrders.Models;

public sealed record AccountOperationResult(
    bool Succeeded,
    string Message,
    string? UserId = null,
    string? Email = null,
    string? Token = null,
    DateTimeOffset? TokenExpiresAt = null,
    IReadOnlyCollection<string>? Errors = null)
{
    public static AccountOperationResult Success(
        string message,
        string? userId = null,
        string? email = null,
        string? token = null,
        DateTimeOffset? tokenExpiresAt = null)
    {
        return new AccountOperationResult(true, message, userId, email, token, tokenExpiresAt, []);
    }

    public static AccountOperationResult Failure(string message, IEnumerable<string>? errors = null)
    {
        return new AccountOperationResult(false, message, Errors: errors?.ToArray() ?? []);
    }
}
