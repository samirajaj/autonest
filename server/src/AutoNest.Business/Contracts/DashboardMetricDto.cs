namespace AutoNest.Business.Contracts;

public sealed record DashboardMetricDto(string Label, decimal Value, string Format = "number");
