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
    private const int MaxImageCount = 10;
    private const long MaxImageBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/gif"
    };

    [HttpGet("dashboard")]
    public Task<DashboardDto> Dashboard(int year = 0, CancellationToken ct = default)
        => management.DashboardAsync(false, year == 0 ? DateTime.UtcNow.Year : year, ct);

    [HttpGet("cars")]
    public async Task<IActionResult> Cars(int page = 1, int pageSize = 12, bool deleted = false, CancellationToken ct = default)
    {
        var result = await cars.CurrentCompanyCarsAsync(page, pageSize, deleted, ct);

        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("cars")]
    public async Task<IActionResult> Create([FromForm] CarForm x, CancellationToken ct)
    {
        var images = await ReadImages(x.Images, ct);

        if (images is null)
        {
            return InvalidImages();
        }

        var id = await cars.CreateAsync(x.ToRequest(), images, ct);

        return id is null
            ? BadRequest(new ProblemDetails { Detail = "Vehicle details are invalid." })
            : Created($"/api/cars/{id}", new { id });
    }

    [HttpPut("cars/{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] CarForm x, CancellationToken ct)
    {
        var images = await ReadImages(x.Images, ct);

        return images is null
            ? InvalidImages()
            : Result(await cars.UpdateAsync(id, x.ToRequest(), images, ct));
    }

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

    private IActionResult Result(OperationResult r)
        => r.Succeeded ? NoContent() : BadRequest(new ProblemDetails { Detail = r.Error });

    private IActionResult InvalidImages()
        => BadRequest(new ProblemDetails
        {
            Detail = $"Upload at most {MaxImageCount} JPEG, PNG, WebP, or GIF images of {MaxImageBytes / 1024 / 1024} MB each."
        });

    private static async Task<IReadOnlyList<(byte[] Data, string ContentType)>?> ReadImages(IReadOnlyList<IFormFile>? files, CancellationToken ct)
    {
        var output = new List<(byte[], string)>();

        if (files is null)
        {
            return output;
        }

        if (files.Count > MaxImageCount)
        {
            return null;
        }

        foreach (var file in files)
        {
            if (file.Length == 0 || file.Length > MaxImageBytes || !AllowedImageTypes.Contains(file.ContentType))
            {
                return null;
            }

            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, ct);
            var data = memory.ToArray();

            if (!HasExpectedSignature(data, file.ContentType))
            {
                return null;
            }

            output.Add((data, file.ContentType));
        }

        return output;
    }

    private static bool HasExpectedSignature(byte[] data, string contentType)
        => contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF,
            "image/png" => data.Length >= 8
                && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47
                && data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A,
            "image/gif" => data.AsSpan().StartsWith("GIF87a"u8) || data.AsSpan().StartsWith("GIF89a"u8),
            "image/webp" => data.Length >= 12
                && data.AsSpan(0, 4).SequenceEqual("RIFF"u8)
                && data.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
}
