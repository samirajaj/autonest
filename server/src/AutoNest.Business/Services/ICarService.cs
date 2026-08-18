using AutoNest.Business.Contracts;

namespace AutoNest.Business.Services;

public interface ICarService
{
    Task<PagedResult<CarSummaryDto>> BrowseAsync(CarFilter filter, CancellationToken ct);
    Task<CarDetailsDto?> GetAsync(int id, CancellationToken ct);
    Task<byte[]?> GetImageAsync(int imageId, CancellationToken ct);
    Task<IReadOnlyList<CompanySummaryDto>> CompaniesAsync(CancellationToken ct);
    Task<PagedResult<CarSummaryDto>> CompanyCarsAsync(int companyId, int page, int pageSize, bool includeDeleted, CancellationToken ct);
    Task<int?> CreateAsync(CarUpsertRequest request, IReadOnlyList<(byte[] Data, string ContentType)> images, CancellationToken ct);
    Task<OperationResult> UpdateAsync(int id, CarUpsertRequest request, IReadOnlyList<(byte[] Data, string ContentType)> images, CancellationToken ct);
    Task<OperationResult> SoftDeleteAsync(int id, CancellationToken ct);
    Task<OperationResult> RestoreAsync(int id, CancellationToken ct);
}
