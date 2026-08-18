using System.Security.Claims;
using AutoNest.Business.Contracts;

namespace AutoNest.Api.Infrastructure;

public sealed class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    public string UserId
        => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();

    public string Role
        => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
}
