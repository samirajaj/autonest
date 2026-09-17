using System.Text;
using System.ComponentModel.DataAnnotations;
using AutoNest.Business.Contracts;
using AutoNest.Data;
using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Business.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> users,
    AutoNestDbContext db,
    ITokenService tokens,
    IEmailService email) : IAuthService
{
    public async Task<OperationResult> RegisterCustomerAsync(RegisterCustomerRequest request, string confirmationBaseUrl, CancellationToken ct)
    {
        if (!IsRegistrationValid(request))
        {
            return OperationResult.Fail("Registration details are invalid.");
        }

        if (!await db.Cities.AnyAsync(x => x.Id == request.CityId, ct))
        {
            return OperationResult.Fail("The selected city does not exist.");
        }

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(ct)
            : null;
        var user = new ApplicationUser
        {
            Email = request.Email.Trim(),
            UserName = request.UserName.Trim()
        };
        var created = await users.CreateAsync(user, request.Password);

        if (!created.Succeeded)
        {
            return OperationResult.Fail(string.Join(" ", created.Errors.Select(x => x.Description)));
        }

        var roleResult = await users.AddToRoleAsync(user, "Customer");

        if (!roleResult.Succeeded)
        {
            await users.DeleteAsync(user);
            return OperationResult.Fail(string.Join(" ", roleResult.Errors.Select(x => x.Description)));
        }

        var address = new Address
        {
            CityId = request.CityId,
            AreaName = request.AreaName.Trim()
        };
        db.Customers.Add(new Customer
        {
            UserId = user.Id,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            Address = address
        });
        await db.SaveChangesAsync(ct);

        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GenerateEmailConfirmationTokenAsync(user)));
        await email.SendAsync(user.Email!, "Confirm your AutoNest account", $"<a href=\"{confirmationBaseUrl}?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}\">Confirm email</a>", ct);
        if (transaction is not null)
        {
            await transaction.CommitAsync(ct);
        }

        return OperationResult.Success();
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());

        if (user is null || user.LockoutEnd > DateTimeOffset.UtcNow || !await users.CheckPasswordAsync(user, request.Password) || !user.EmailConfirmed)
        {
            return null;
        }

        var role = (await users.GetRolesAsync(user)).FirstOrDefault() ?? "Customer";
        var display = role == "Company"
            ? await db.Companies.Where(x => x.UserId == user.Id).Select(x => x.Name).FirstOrDefaultAsync() ?? user.UserName!
            : await db.Customers.Where(x => x.UserId == user.Id).Select(x => x.FirstName + " " + x.LastName).FirstOrDefaultAsync() ?? user.UserName!;

        return tokens.CreateToken(user, role, display);
    }

    public async Task<OperationResult> ConfirmEmailAsync(string userId, string token)
    {
        var user = await users.FindByIdAsync(userId);

        if (user is null)
        {
            return OperationResult.Fail("Account not found.");
        }

        if (!TryDecodeToken(token, out var decoded))
        {
            return OperationResult.Fail("The confirmation link is invalid or expired.");
        }

        var result = await users.ConfirmEmailAsync(user, decoded);

        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Fail("The confirmation link is invalid or expired.");
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, string resetBaseUrl, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());

        if (user is null || !user.EmailConfirmed)
        {
            return;
        }

        var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await users.GeneratePasswordResetTokenAsync(user)));
        await email.SendAsync(user.Email!, "Reset your AutoNest password", $"<a href=\"{resetBaseUrl}?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}\">Reset password</a>", ct);
    }

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());

        if (user is null)
        {
            return OperationResult.Fail("Invalid reset request.");
        }

        if (!TryDecodeToken(request.Token, out var decoded))
        {
            return OperationResult.Fail("Invalid reset request.");
        }

        var result = await users.ResetPasswordAsync(user, decoded, request.NewPassword);

        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Fail(string.Join(" ", result.Errors.Select(x => x.Description)));
    }

    private static bool TryDecodeToken(string token, out string decoded)
    {
        try
        {
            decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            return true;
        }
        catch (FormatException)
        {
            decoded = string.Empty;
            return false;
        }
    }

    private static bool IsRegistrationValid(RegisterCustomerRequest request)
        => !string.IsNullOrWhiteSpace(request.Email)
            && request.Email.Length <= 256
            && new EmailAddressAttribute().IsValid(request.Email)
            && !string.IsNullOrWhiteSpace(request.UserName)
            && !string.IsNullOrWhiteSpace(request.Password)
            && !string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName.Length <= 80
            && !string.IsNullOrWhiteSpace(request.LastName) && request.LastName.Length <= 80
            && !string.IsNullOrWhiteSpace(request.AreaName) && request.AreaName.Length <= 180;
}
