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

        Assert.True(DomainRules.IsRentalPeriodValid(start, start.AddDays(2), start));
        Assert.False(DomainRules.IsRentalPeriodValid(start, start, start));
        Assert.False(DomainRules.IsRentalPeriodValid(null, start, start));
        Assert.False(DomainRules.IsRentalPeriodValid(start.AddDays(-1), start.AddDays(1), start));
    }

    [Theory]
    [InlineData(RequestType.Sale, true, true)]
    [InlineData(RequestType.Rent, false, true)]
    [InlineData(RequestType.Sale, false, false)]
    [InlineData(RequestType.Rent, true, false)]
    public void Request_type_must_match_listing(RequestType type, bool isForSale, bool expected)
        => Assert.Equal(expected, DomainRules.RequestMatchesListing(type, isForSale));

    [Fact]
    public void Approval_requires_positive_payment_and_non_past_deadline()
    {
        var now = new DateTime(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(DomainRules.IsApprovalValid(now.Date, 1, now));
        Assert.False(DomainRules.IsApprovalValid(now.AddDays(-1), 1, now));
        Assert.False(DomainRules.IsApprovalValid(now.AddDays(1), 0, now));
    }

    [Theory]
    [InlineData(RequestState.Pending, true)]
    [InlineData(RequestState.Approved, false)]
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
