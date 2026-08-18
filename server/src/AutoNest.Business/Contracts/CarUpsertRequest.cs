using AutoNest.Data.Entities;

namespace AutoNest.Business.Contracts;

public sealed record CarUpsertRequest(
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
    bool IsForSale);
