namespace AutoNest.Business.Contracts;

public interface ITokenService
{
    AuthResponse CreateToken(AutoNest.Data.Entities.ApplicationUser user, string role, string displayName);
}
