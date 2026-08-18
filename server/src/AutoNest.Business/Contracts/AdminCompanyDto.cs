namespace AutoNest.Business.Contracts;

public sealed record AdminCompanyDto(int Id, string UserId, string Name, string Email, bool Locked, int? PlanId);
