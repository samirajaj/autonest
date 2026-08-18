using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/favorites")]
public sealed class FavoritesController(ICustomerService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CarSummaryDto>> All(CancellationToken ct)
        => service.FavoritesAsync(ct);

    [HttpPost("{carId:int}")]
    public async Task<IActionResult> Add(int carId, CancellationToken ct)
    {
        var r = await service.AddFavoriteAsync(carId, ct);

        return r.Succeeded ? NoContent() : BadRequest(new ProblemDetails { Detail = r.Error });
    }

    [HttpDelete("{carId:int}")]
    public async Task<IActionResult> Remove(int carId, CancellationToken ct)
    {
        var r = await service.RemoveFavoriteAsync(carId, ct);

        return r.Succeeded ? NoContent() : NotFound(new ProblemDetails { Detail = r.Error });
    }
}
