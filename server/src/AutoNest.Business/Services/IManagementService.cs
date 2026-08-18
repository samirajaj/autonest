using AutoNest.Business.Contracts;
using AutoNest.Data.Entities;

namespace AutoNest.Business.Services;

public interface IManagementService
{
    Task<DashboardDto> DashboardAsync(bool admin, int year, CancellationToken ct);
    Task<PagedResult<RequestDto>> CompanyRequestsAsync(RequestState? state, int page, int size, CancellationToken ct);
    Task<OperationResult> ApproveAsync(int id, ApproveRequestDto input, CancellationToken ct);
    Task<OperationResult> RejectAsync(int id, CancellationToken ct);
    Task<PagedResult<AdminCustomerDto>> CustomersAsync(int page, int size, CancellationToken ct);
    Task<PagedResult<AdminCompanyDto>> CompaniesAsync(int page, int size, CancellationToken ct);
    Task<OperationResult> SetLockAsync(string userId, bool locked);
    Task<OperationResult> CreateCustomerAsync(AdminCustomerUpsertRequest input, CancellationToken ct);
    Task<OperationResult> CreateCompanyAsync(AdminCompanyUpsertRequest input, CancellationToken ct);
    Task<OperationResult> UpdateCustomerAsync(int id, AdminCustomerUpsertRequest input, CancellationToken ct);
    Task<OperationResult> UpdateCompanyAsync(int id, AdminCompanyUpsertRequest input, CancellationToken ct);
    Task<OperationResult> ChangePasswordAsync(string userId, string password);
    Task<IReadOnlyList<PlanDto>> PlansAsync(CancellationToken ct);
    Task<OperationResult> SavePlanAsync(int? id, PlanUpsertRequest input, CancellationToken ct);
    Task<OperationResult> DeletePlanAsync(int id, CancellationToken ct);
    Task<OperationResult> AssignPlanAsync(int companyId, int planId, CancellationToken ct);
    Task<IReadOnlyList<PointRangeDto>> PointRangesAsync(CancellationToken ct);
    Task<OperationResult> SavePointRangeAsync(int? id, PointRangeUpsertRequest input, CancellationToken ct);
    Task<OperationResult> DeletePointRangeAsync(int id, CancellationToken ct);
}
