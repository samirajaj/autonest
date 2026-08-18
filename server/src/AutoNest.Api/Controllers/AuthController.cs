using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, IConfiguration configuration) : ControllerBase
{
    private string ClientBaseUrl =>
        (configuration["ClientBaseUrl"]
            ?? throw new InvalidOperationException("ClientBaseUrl is required."))
        .TrimEnd('/');

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCustomerRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterCustomerAsync(request, ClientBaseUrl + "/auth/confirm-email", ct);

        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created)
            : BadRequest(new ProblemDetails
            {
                Title = "Registration failed",
                Detail = result.Error
            });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await auth.LoginAsync(request);

        return response is null
            ? Unauthorized(new ProblemDetails
            {
                Title = "Invalid credentials",
                Detail = "Check your credentials and confirm your email."
            })
            : Ok(response);
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        var result = await auth.ConfirmEmailAsync(userId, token);

        return result.Succeeded
            ? NoContent()
            : BadRequest(new ProblemDetails
            {
                Title = "Confirmation failed",
                Detail = result.Error
            });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await auth.ForgotPasswordAsync(request, ClientBaseUrl + "/auth/reset-password", ct);

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var result = await auth.ResetPasswordAsync(request);

        return result.Succeeded
            ? NoContent()
            : BadRequest(new ProblemDetails
            {
                Title = "Reset failed",
                Detail = result.Error
            });
    }
}
