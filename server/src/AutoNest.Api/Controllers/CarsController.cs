using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AutoNest.Api.Controllers;

[ApiController]
[Route("api/cars")]
public sealed class CarsController(ICarService cars) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<CarSummaryDto>> Browse(
        [FromQuery] string? search,
        [FromQuery] string? make,
        [FromQuery] CarType? type,
        [FromQuery] GearType? gearType,
        [FromQuery] FuelType? fuelType,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? minYear,
        [FromQuery] int? maxYear,
        [FromQuery] bool? isForSale,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
        => cars.BrowseAsync(new(search, make, type, gearType, fuelType, minPrice, maxPrice, minYear, maxYear, isForSale, page, pageSize), ct);

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var car = await cars.GetAsync(id, ct);

        return car is null ? NotFound() : Ok(car);
    }

    [HttpGet("images/{id:int}")]
    public async Task<IActionResult> Image(int id, CancellationToken ct)
    {
        var image = await cars.GetImageAsync(id, ct);

        return image is null ? NotFound() : File(image, "image/jpeg");
    }
}
