using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Route("api/cities")]
public sealed class CitiesController(ILookupService lookups) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CityDto>> All(CancellationToken ct)
        => lookups.CitiesAsync(ct);
}
