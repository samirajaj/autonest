using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class CarImage
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public byte[] Image { get; set; } = [];

    [MaxLength(100)]
    public string ContentType { get; set; } = "image/jpeg";
}
