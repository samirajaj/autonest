namespace AutoNest.Business.Contracts;

public sealed record DashboardDto(IReadOnlyList<DashboardMetricDto> Metrics, decimal[] QuarterlyProfits);
