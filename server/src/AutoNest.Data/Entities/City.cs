using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class City
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Address> Addresses { get; set; } = [];
}
