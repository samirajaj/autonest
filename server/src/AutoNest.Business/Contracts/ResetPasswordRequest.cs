namespace AutoNest.Business.Contracts;

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
