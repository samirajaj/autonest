using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController(ICarService cars) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CompanySummaryDto>> All(CancellationToken ct)
        => cars.CompaniesAsync(ct);

    [HttpGet("{id:int}/cars")]
    public Task<PagedResult<CarSummaryDto>> Cars(int id, int page = 1, int pageSize = 12, CancellationToken ct = default)
        => cars.CompanyCarsAsync(id, page, pageSize, false, ct);
}
