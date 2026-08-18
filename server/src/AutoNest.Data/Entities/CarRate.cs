namespace AutoNest.Data.Entities;

public sealed class CarRate
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public decimal Value { get; set; }
}
