using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class Plan
{
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int Duration { get; set; }
    public decimal Price { get; set; }
    public ICollection<SubscriptionPlan> Subscriptions { get; set; } = [];
}
