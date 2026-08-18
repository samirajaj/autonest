using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Authorize(Roles = "Company")]
[Route("api/company")]
public sealed class CompanyController(IManagementService management, ICarService cars, ICustomerService account) : ControllerBase
{
    [HttpGet("dashboard")]
    public Task<DashboardDto> Dashboard(int year = 0, CancellationToken ct = default)
        => management.DashboardAsync(false, year == 0 ? DateTime.UtcNow.Year : year, ct);

    [HttpGet("cars")]
    public async Task<IActionResult> Cars(int page = 1, int pageSize = 12, bool deleted = false, CancellationToken ct = default)
    {
        var companyId = await CurrentCompanyId();

        return companyId is null
            ? NotFound()
            : Ok(await cars.CompanyCarsAsync(companyId.Value, page, pageSize, deleted, ct));
    }

    [HttpPost("cars")]
    public async Task<IActionResult> Create([FromForm] CarForm x, CancellationToken ct)
    {
        var images = await ReadImages(x.Images, ct);
        var id = await cars.CreateAsync(x.ToRequest(), images, ct);

        return id is null
            ? BadRequest(new ProblemDetails { Detail = "Vehicle details are invalid." })
            : Created($"/api/cars/{id}", new { id });
    }

    [HttpPut("cars/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] CarForm x, CancellationToken ct)
        => Result(await cars.UpdateAsync(id, x.ToRequest(), await ReadImages(x.Images, ct), ct));

    [HttpDelete("cars/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => Result(await cars.SoftDeleteAsync(id, ct));

    [HttpPut("cars/{id:int}/restore")]
    public async Task<IActionResult> Restore(int id, CancellationToken ct)
        => Result(await cars.RestoreAsync(id, ct));

    [HttpGet("requests")]
    public Task<PagedResult<RequestDto>> Requests(RequestState? state, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => management.CompanyRequestsAsync(state, page, pageSize, ct);

    [HttpPut("requests/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, ApproveRequestDto x, CancellationToken ct)
        => Result(await management.ApproveAsync(id, x, ct));

    [HttpPut("requests/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
        => Result(await management.RejectAsync(id, ct));

    [HttpPut("password")]
    public async Task<IActionResult> Password(ChangePasswordRequest x)
        => Result(await account.ChangePasswordAsync(x));

    private async Task<int?> CurrentCompanyId()
    {
        var context = HttpContext.RequestServices.GetRequiredService<AutoNest.Data.AutoNestDbContext>();
        var user = HttpContext.RequestServices.GetRequiredService<IUserContext>();

        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(context.Companies.Where(x => x.UserId == user.UserId).Select(x => (int?)x.Id));
    }

    private IActionResult Result(OperationResult r)
        => r.Succeeded ? NoContent() : BadRequest(new ProblemDetails { Detail = r.Error });

    private static async Task<IReadOnlyList<(byte[] Data, string ContentType)>> ReadImages(IReadOnlyList<IFormFile>? files, CancellationToken ct)
    {
        var output = new List<(byte[], string)>();

        foreach (var file in files ?? [])
        {
            if (file.Length == 0 || file.Length > 10 * 1024 * 1024 || !file.ContentType.StartsWith("image/"))
            {
                continue;
            }

            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, ct);
            output.Add((memory.ToArray(), file.ContentType));
        }

        return output;
    }
}
