using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/requests")]
public sealed class RequestsController(ICustomerService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<RequestDto>> All(CancellationToken ct)
        => service.RequestsAsync(ct);

    [HttpPost]
    public async Task<IActionResult> Create(CreateRequestDto x, CancellationToken ct)
    {
        var r = await service.CreateRequestAsync(x, ct);

        return r.Succeeded ? StatusCode(201) : BadRequest(new ProblemDetails { Detail = r.Error });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var r = await service.CancelRequestAsync(id, ct);

        return r.Succeeded ? NoContent() : BadRequest(new ProblemDetails { Detail = r.Error });
    }
}
