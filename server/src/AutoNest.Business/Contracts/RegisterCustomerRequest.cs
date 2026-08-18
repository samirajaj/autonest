namespace AutoNest.Business.Contracts;

public sealed record RegisterCustomerRequest(
    string Email,
    string UserName,
    string Password,
    string FirstName,
    string LastName,
    DateOnly BirthDate,
    int CityId,
    string AreaName);
