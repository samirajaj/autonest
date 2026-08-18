using AutoNest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Api.Controllers;

[ApiController]
[Route("api/cities")]
public sealed class CitiesController(AutoNestDbContext db) : ControllerBase
{
    [HttpGet]
    public Task<List<object>> All(CancellationToken ct)
        => db.Cities.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new { x.Id, x.Name })
            .Cast<object>()
            .ToListAsync(ct);
}
