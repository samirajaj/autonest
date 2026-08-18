using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class Address
{
    public int Id { get; set; }
    public int CityId { get; set; }
    public City City { get; set; } = null!;

    [MaxLength(180)]
    public string AreaName { get; set; } = string.Empty;
}
