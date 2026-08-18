namespace AutoNest.Business.Contracts;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
