using AutoNest.Data.Entities;

namespace AutoNest.Business.Contracts;

public sealed record CarSummaryDto(
    int Id,
    int CompanyId,
    string Company,
    string Make,
    string Model,
    int Year,
    CarType Type,
    GearType GearType,
    FuelType FuelType,
    int SeatsCount,
    int Mileage,
    decimal Price,
    bool IsAvailable,
    bool IsForSale,
    bool InRent,
    decimal Rating,
    string? ImageUrl);
