namespace AutoNest.Data.Entities;

public sealed class SubscriptionPlan
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public decimal PaidAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
