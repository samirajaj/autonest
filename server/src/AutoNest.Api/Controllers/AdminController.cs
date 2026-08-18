using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public sealed class AdminController(IManagementService service, ICarService cars) : ControllerBase
{
    [HttpGet("dashboard")]
    public Task<DashboardDto> Dashboard(int year = 0, CancellationToken ct = default)
        => service.DashboardAsync(true, year == 0 ? DateTime.UtcNow.Year : year, ct);

    [HttpGet("customers")]
    public Task<PagedResult<AdminCustomerDto>> Customers(int page = 1, int pageSize = 20, CancellationToken ct = default)
        => service.CustomersAsync(page, pageSize, ct);

    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer(AdminCustomerUpsertRequest x, CancellationToken ct)
        => Result(await service.CreateCustomerAsync(x, ct), true);

    [HttpPut("customers/{id:int}")]
    public async Task<IActionResult> UpdateCustomer(int id, AdminCustomerUpsertRequest x, CancellationToken ct)
        => Result(await service.UpdateCustomerAsync(id, x, ct));

    [HttpGet("companies")]
    public Task<PagedResult<AdminCompanyDto>> Companies(int page = 1, int pageSize = 20, CancellationToken ct = default)
        => service.CompaniesAsync(page, pageSize, ct);

    [HttpPost("companies")]
    public async Task<IActionResult> CreateCompany(AdminCompanyUpsertRequest x, CancellationToken ct)
        => Result(await service.CreateCompanyAsync(x, ct), true);

    [HttpPut("companies/{id:int}")]
    public async Task<IActionResult> UpdateCompany(int id, AdminCompanyUpsertRequest x, CancellationToken ct)
        => Result(await service.UpdateCompanyAsync(id, x, ct));

    [HttpGet("companies/{id:int}/cars")]
    public Task<PagedResult<CarSummaryDto>> CompanyCars(int id, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => cars.CompanyCarsAsync(id, page, pageSize, false, ct);

    [HttpPut("users/{userId}/lock")]
    public async Task<IActionResult> Lock(string userId)
        => Result(await service.SetLockAsync(userId, true));

    [HttpDelete("users/{userId}/lock")]
    public async Task<IActionResult> Unlock(string userId)
        => Result(await service.SetLockAsync(userId, false));

    [HttpPut("users/{userId}/password")]
    public async Task<IActionResult> Password(string userId, AdminPasswordRequest x)
        => Result(await service.ChangePasswordAsync(userId, x.NewPassword));

    [HttpGet("plans")]
    public Task<IReadOnlyList<PlanDto>> Plans(CancellationToken ct)
        => service.PlansAsync(ct);

    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan(PlanUpsertRequest x, CancellationToken ct)
        => Result(await service.SavePlanAsync(null, x, ct), true);

    [HttpPut("plans/{id:int}")]
    public async Task<IActionResult> UpdatePlan(int id, PlanUpsertRequest x, CancellationToken ct)
        => Result(await service.SavePlanAsync(id, x, ct));

    [HttpDelete("plans/{id:int}")]
    public async Task<IActionResult> DeletePlan(int id, CancellationToken ct)
        => Result(await service.DeletePlanAsync(id, ct));

    [HttpPut("companies/{companyId:int}/plan/{planId:int}")]
    public async Task<IActionResult> AssignPlan(int companyId, int planId, CancellationToken ct)
        => Result(await service.AssignPlanAsync(companyId, planId, ct));

    [HttpGet("point-ranges")]
    public Task<IReadOnlyList<PointRangeDto>> PointRanges(CancellationToken ct)
        => service.PointRangesAsync(ct);

    [HttpPost("point-ranges")]
    public async Task<IActionResult> CreateRange(PointRangeUpsertRequest x, CancellationToken ct)
        => Result(await service.SavePointRangeAsync(null, x, ct), true);

    [HttpPut("point-ranges/{id:int}")]
    public async Task<IActionResult> UpdateRange(int id, PointRangeUpsertRequest x, CancellationToken ct)
        => Result(await service.SavePointRangeAsync(id, x, ct));

    [HttpDelete("point-ranges/{id:int}")]
    public async Task<IActionResult> DeleteRange(int id, CancellationToken ct)
        => Result(await service.DeletePointRangeAsync(id, ct));

    private IActionResult Result(OperationResult x, bool created = false)
        => x.Succeeded
            ? created ? StatusCode(201) : NoContent()
            : BadRequest(new ProblemDetails { Detail = x.Error });
}
