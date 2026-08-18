namespace AutoNest.Business.Contracts;

public sealed record CustomerProfileDto(
    int Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    string City,
    string Area,
    int Points);
