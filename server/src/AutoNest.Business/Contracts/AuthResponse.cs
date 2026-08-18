namespace AutoNest.Business.Contracts;

public sealed record AuthResponse(string Token, DateTime ExpiresAt, string Role, string DisplayName);
