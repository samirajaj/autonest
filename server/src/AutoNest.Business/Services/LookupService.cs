using AutoNest.Business.Contracts;
using AutoNest.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Business.Services;

public sealed class LookupService(AutoNestDbContext db) : ILookupService
{
    public async Task<IReadOnlyList<CityDto>> CitiesAsync(CancellationToken ct)
        => await db.Cities.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CityDto(x.Id, x.Name))
            .ToListAsync(ct);
}
