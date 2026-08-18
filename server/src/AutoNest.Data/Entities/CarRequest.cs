namespace AutoNest.Data.Entities;

public sealed class CarRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int CarId { get; set; }
    public Car Car { get; set; } = null!;
    public RequestState State { get; set; } = RequestState.Pending;
    public RequestType Type { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Transaction? Transaction { get; set; }
}
