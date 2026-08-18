using AutoNest.Business.Contracts;
using AutoNest.Data.Entities;

namespace AutoNest.Business.Tests;

public sealed class DomainRulesTests
{
    [Fact]
    public void Vehicle_requires_valid_core_values()
    {
        var valid = new CarUpsertRequest("Toyota", "Corolla", 2024, CarType.Sedan, GearType.Automatic, FuelType.Hybrid, 5, 1000, 24000, true, true);

        Assert.True(DomainRules.IsVehicleValid(valid));
        Assert.False(DomainRules.IsVehicleValid(valid with { Price = -1 }));
        Assert.False(DomainRules.IsVehicleValid(valid with { Make = "" }));
    }

    [Fact]
    public void Rental_must_end_after_start()
    {
        var start = new DateTime(2026, 8, 1);

        Assert.True(DomainRules.IsRentalPeriodValid(start, start.AddDays(2)));
        Assert.False(DomainRules.IsRentalPeriodValid(start, start));
        Assert.False(DomainRules.IsRentalPeriodValid(null, start));
    }

    [Theory]
    [InlineData(RequestState.Pending, true)]
    [InlineData(RequestState.Approved, true)]
    [InlineData(RequestState.Completed, false)]
    [InlineData(RequestState.Rejected, false)]
    public void Cancellation_follows_request_state(RequestState state, bool expected)
        => Assert.Equal(expected, DomainRules.CanCancel(state));

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void Rating_is_one_through_five(decimal value, bool expected)
        => Assert.Equal(expected, DomainRules.IsRatingValid(value));

    [Fact]
    public void Point_ranges_detect_overlap()
    {
        Assert.True(DomainRules.RangesOverlap(0, 100, 100, 200));
        Assert.False(DomainRules.RangesOverlap(0, 99, 100, 200));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(4, 1)]
    [InlineData(7, 2)]
    [InlineData(12, 3)]
    public void Month_maps_to_quarter(int month, int quarter)
        => Assert.Equal(quarter, DomainRules.Quarter(new DateTime(2026, month, 1)));
}
