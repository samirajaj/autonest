namespace AutoNest.Business.Contracts;

public sealed record AdminCompanyUpsertRequest(
    string Email,
    string UserName,
    string? Password,
    string Name,
    int CityId,
    string AreaName,
    IReadOnlyList<ContactDto> Contacts);
