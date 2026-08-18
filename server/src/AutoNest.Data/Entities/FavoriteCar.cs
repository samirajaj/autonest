namespace AutoNest.Data.Entities;

public sealed class FavoriteCar
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
}
