namespace AutoNest.Business.Contracts;

public sealed record PlanUpsertRequest(string Name, int Duration, decimal Price);
