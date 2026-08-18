using AutoNest.Business.Contracts;
using AutoNest.Data.Entities;

namespace AutoNest.Business;

public static class DomainRules
{
    public static bool IsVehicleValid(CarUpsertRequest x)
        => !string.IsNullOrWhiteSpace(x.Make)
            && !string.IsNullOrWhiteSpace(x.Model)
            && x.Year is >= 1900 and <= 2100
            && x.SeatsCount > 0
            && x.Price >= 0
            && x.Mileage >= 0;

    public static bool IsRentalPeriodValid(DateTime? start, DateTime? end)
        => start.HasValue && end.HasValue && end.Value.Date > start.Value.Date;

    public static bool CanCancel(RequestState state)
        => state is RequestState.Pending or RequestState.Approved;

    public static bool CanApprove(RequestState state)
        => state == RequestState.Pending;

    public static bool IsRatingValid(decimal value)
        => value is >= 1 and <= 5;

    public static bool RangesOverlap(int minA, int maxA, int minB, int maxB)
        => minA <= maxB && maxA >= minB;

    public static int Quarter(DateTime date)
        => (date.Month - 1) / 3;
}
