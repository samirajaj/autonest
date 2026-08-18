using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Authorize(Roles = "Customer")]
[Route("api/transactions")]
public sealed class TransactionsController(ICustomerService service) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<TransactionDto>> All(CancellationToken ct)
        => service.TransactionsAsync(ct);

    [HttpPut("{id:int}/rating")]
    public async Task<IActionResult> Rate(int id, RateTransactionDto x, CancellationToken ct)
    {
        var r = await service.RateAsync(id, x.Value, ct);

        return r.Succeeded ? NoContent() : BadRequest(new ProblemDetails { Detail = r.Error });
    }
}
