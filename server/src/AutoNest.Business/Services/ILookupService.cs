using AutoNest.Business.Contracts;

namespace AutoNest.Business.Services;

public interface ILookupService
{
    Task<IReadOnlyList<CityDto>> CitiesAsync(CancellationToken ct);
}
