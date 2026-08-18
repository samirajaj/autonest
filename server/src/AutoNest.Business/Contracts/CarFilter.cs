using AutoNest.Data.Entities;

namespace AutoNest.Business.Contracts;

public sealed record CarFilter(
    string? Search,
    string? Make,
    CarType? Type,
    GearType? GearType,
    FuelType? FuelType,
    decimal? MinPrice,
    decimal? MaxPrice,
    int? MinYear,
    int? MaxYear,
    bool? IsForSale,
    int Page = 1,
    int PageSize = 12);
