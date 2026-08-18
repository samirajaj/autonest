using AutoNest.Business.Contracts;

namespace AutoNest.Business.Services;

public interface IAuthService
{
    Task<OperationResult> RegisterCustomerAsync(RegisterCustomerRequest request, string confirmationBaseUrl, CancellationToken ct);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<OperationResult> ConfirmEmailAsync(string userId, string token);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, string resetBaseUrl, CancellationToken ct);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request);
}
