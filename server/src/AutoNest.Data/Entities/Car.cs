using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class Car
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [MaxLength(100)]
    public string Make { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }
    public CarType Type { get; set; }
    public GearType GearType { get; set; }
    public FuelType FuelType { get; set; }
    public int SeatsCount { get; set; }
    public int Mileage { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsForSale { get; set; }
    public bool InRent { get; set; }
    public DateTime ListingDate { get; set; } = DateTime.UtcNow;
    public DateTime? SoldAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public ICollection<CarImage> Images { get; set; } = [];
    public ICollection<CarRate> Rates { get; set; } = [];
}
