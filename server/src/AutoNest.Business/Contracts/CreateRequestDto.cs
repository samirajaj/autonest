using AutoNest.Data.Entities;

namespace AutoNest.Business.Contracts;

public sealed record CreateRequestDto(int CarId, RequestType Type, DateTime? StartDate, DateTime? EndDate);
