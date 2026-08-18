namespace AutoNest.Business.Contracts;

public sealed record PointRangeUpsertRequest(int MinAmount, int MaxAmount, int Point);
