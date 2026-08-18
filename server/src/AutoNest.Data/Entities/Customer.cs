using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class Customer
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }
    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int Point { get; set; }
    public ICollection<FavoriteCar> Favorites { get; set; } = [];
    public ICollection<CarRequest> Requests { get; set; } = [];
}
