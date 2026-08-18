using System.ComponentModel.DataAnnotations;

namespace AutoNest.Data.Entities;

public sealed class Company
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;
    public int? SubscriptionPlanId { get; set; }
    public SubscriptionPlan? SubscriptionPlan { get; set; }

    [MaxLength(180)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    public DateTime? UpdatedAt { get; set; }
    public ICollection<Car> Cars { get; set; } = [];
    public ICollection<Contact> Contacts { get; set; } = [];
}
