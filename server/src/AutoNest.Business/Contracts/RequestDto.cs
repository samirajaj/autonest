using AutoNest.Data.Entities;

namespace AutoNest.Business.Contracts;

public sealed record RequestDto(
    int Id,
    int CarId,
    string Car,
    string Company,
    RequestType Type,
    RequestState State,
    DateTime RequestDate,
    DateTime? Deadline,
    DateTime? StartDate,
    DateTime? EndDate);
