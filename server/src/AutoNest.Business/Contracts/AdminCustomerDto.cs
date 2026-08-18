namespace AutoNest.Business.Contracts;

public sealed record AdminCustomerDto(int Id, string UserId, string Name, string Email, bool Locked, int Points);
