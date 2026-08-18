using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/profile")]
public sealed class ProfileController(ICustomerService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var profile = await service.ProfileAsync(ct);

        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPut("username")]
    public async Task<IActionResult> Username(ChangeUsernameRequest x)
        => ApiResponse(await service.ChangeUsernameAsync(x));

    [HttpPut("email")]
    public async Task<IActionResult> Email(ChangeEmailRequest x)
        => ApiResponse(await service.ChangeEmailAsync(x));

    [HttpPut("password")]
    public async Task<IActionResult> Password(ChangePasswordRequest x)
        => ApiResponse(await service.ChangePasswordAsync(x));

    [HttpDelete]
    public async Task<IActionResult> Delete(DeleteAccountRequest x, CancellationToken ct)
        => ApiResponse(await service.DeleteAccountAsync(x, ct));

    private IActionResult ApiResponse(OperationResult x)
        => x.Succeeded
            ? NoContent()
            : BadRequest(new ProblemDetails
            {
                Title = "Profile update failed",
                Detail = x.Error
            });
}
