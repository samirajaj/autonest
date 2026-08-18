namespace AutoNest.Business.Contracts;

public sealed record OperationResult(bool Succeeded, string? Error = null)
{
    public static OperationResult Success() => new(true);
    public static OperationResult Fail(string error) => new(false, error);
}
