namespace AutoNest.Data.Entities;

public sealed class Transaction
{
    public int Id { get; set; }
    public int RequestId { get; set; }
    public CarRequest Request { get; set; } = null!;
    public decimal PaidAmount { get; set; }
    public DateTime ListingDate { get; set; } = DateTime.UtcNow;
    public TransactionStatus State { get; set; }
    public CarRate? Rating { get; set; }
}
