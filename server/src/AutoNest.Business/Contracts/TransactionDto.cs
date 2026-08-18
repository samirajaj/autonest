using AutoNest.Data.Entities;

namespace AutoNest.Business.Contracts;

public sealed record TransactionDto(
    int Id,
    int RequestId,
    string Car,
    decimal PaidAmount,
    DateTime ListingDate,
    TransactionStatus State,
    decimal? Rating);
