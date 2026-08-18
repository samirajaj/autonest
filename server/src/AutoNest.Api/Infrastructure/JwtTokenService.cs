using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoNest.Business.Contracts;
using AutoNest.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace AutoNest.Api.Infrastructure;

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public AuthResponse CreateToken(ApplicationUser user, string role, string displayName)
    {
        var expires = DateTime.UtcNow.AddMinutes(configuration.GetValue("Jwt:Minutes", 120));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Role, role),
            new Claim("display_name", displayName)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: credentials);

        return new(new JwtSecurityTokenHandler().WriteToken(token), expires, role, displayName);
    }
}
