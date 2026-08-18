using AutoNest.Business.Contracts;
using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Http;

namespace AutoNest.Api.Controllers;

public sealed class CarForm
{
    public string Make { get; set; } = "";
    public string Model { get; set; } = "";
    public int Year { get; set; }
    public CarType Type { get; set; }
    public GearType GearType { get; set; }
    public FuelType FuelType { get; set; }
    public int SeatsCount { get; set; }
    public int Mileage { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsForSale { get; set; }
    public List<IFormFile>? Images { get; set; }

    public CarUpsertRequest ToRequest()
        => new(Make, Model, Year, Type, GearType, FuelType, SeatsCount, Mileage, Price, IsAvailable, IsForSale);
}
