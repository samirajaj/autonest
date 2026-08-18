using AutoNest.Business.Contracts;
using AutoNest.Data;
using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Business.Services;

public sealed class CustomerService(
    AutoNestDbContext db,
    IUserContext current,
    UserManager<ApplicationUser> users) : ICustomerService
{
    public Task<CustomerProfileDto?> ProfileAsync(CancellationToken ct)
        => db.Customers.AsNoTracking()
            .Where(x => x.UserId == current.UserId)
            .Select(x => new CustomerProfileDto(
                x.Id,
                x.User.UserName!,
                x.User.Email!,
                x.FirstName,
                x.LastName,
                x.BirthDate,
                x.Address.City.Name,
                x.Address.AreaName,
                x.Point))
            .FirstOrDefaultAsync(ct);

    public async Task<OperationResult> ChangeUsernameAsync(ChangeUsernameRequest request)
    {
        var user = await users.FindByIdAsync(current.UserId);

        if (user is null)
        {
            return OperationResult.Fail("Account not found.");
        }

        var result = await users.SetUserNameAsync(user, request.UserName.Trim());

        return IdentityResult(result);
    }

    public async Task<OperationResult> ChangeEmailAsync(ChangeEmailRequest request)
    {
        var user = await users.FindByIdAsync(current.UserId);

        if (user is null)
        {
            return OperationResult.Fail("Account not found.");
        }

        var token = await users.GenerateChangeEmailTokenAsync(user, request.NewEmail.Trim());
        var result = await users.ChangeEmailAsync(user, request.NewEmail.Trim(), token);

        return IdentityResult(result);
    }

    public async Task<OperationResult> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await users.FindByIdAsync(current.UserId);

        if (user is null)
        {
            return OperationResult.Fail("Account not found.");
        }

        return IdentityResult(await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword));
    }

    public async Task<OperationResult> DeleteAccountAsync(DeleteAccountRequest request, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(current.UserId);

        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
        {
            return OperationResult.Fail("Password is incorrect.");
        }

        var customer = await db.Customers.FirstOrDefaultAsync(x => x.UserId == current.UserId, ct);

        if (customer is null)
        {
            return OperationResult.Fail("Customer not found.");
        }

        var hasTransactions = await db.Transactions.AnyAsync(x => x.Request.CustomerId == customer.Id, ct);

        if (hasTransactions)
        {
            return OperationResult.Fail("Accounts with transaction history cannot be deleted.");
        }

        db.Customers.Remove(customer);
        await db.SaveChangesAsync(ct);

        return IdentityResult(await users.DeleteAsync(user));
    }

    public async Task<IReadOnlyList<CarSummaryDto>> FavoritesAsync(CancellationToken ct)
    {
        var id = await CustomerId(ct);

        if (id is null)
        {
            return [];
        }

        return await db.FavoriteCars.AsNoTracking()
            .Where(x => x.CustomerId == id && x.Car.DeletedAt == null)
            .Select(x => new CarSummaryDto(
                x.Car.Id,
                x.Car.CompanyId,
                x.Car.Company.Name,
                x.Car.Make,
                x.Car.Model,
                x.Car.Year,
                x.Car.Type,
                x.Car.GearType,
                x.Car.FuelType,
                x.Car.SeatsCount,
                x.Car.Mileage,
                x.Car.Price,
                x.Car.IsAvailable,
                x.Car.IsForSale,
                x.Car.InRent,
                x.Car.Rates.Any() ? x.Car.Rates.Average(r => r.Value) : 0,
                x.Car.Images.Select(i => $"/api/cars/images/{i.Id}").FirstOrDefault()))
            .ToListAsync(ct);
    }

    public async Task<OperationResult> AddFavoriteAsync(int carId, CancellationToken ct)
    {
        var id = await CustomerId(ct);

        if (id is null || !await db.Cars.AnyAsync(x => x.Id == carId && x.DeletedAt == null, ct))
        {
            return OperationResult.Fail("Vehicle not found.");
        }

        if (!await db.FavoriteCars.AnyAsync(x => x.CustomerId == id && x.CarId == carId, ct))
        {
            db.FavoriteCars.Add(new FavoriteCar { CustomerId = id.Value, CarId = carId });
            await db.SaveChangesAsync(ct);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> RemoveFavoriteAsync(int carId, CancellationToken ct)
    {
        var id = await CustomerId(ct);
        var favorite = await db.FavoriteCars.FirstOrDefaultAsync(x => x.CustomerId == id && x.CarId == carId, ct);

        if (favorite is null)
        {
            return OperationResult.Fail("Favorite not found.");
        }

        db.FavoriteCars.Remove(favorite);
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<RequestDto>> RequestsAsync(CancellationToken ct)
    {
        var id = await CustomerId(ct);

        return await db.Requests.AsNoTracking()
            .Where(x => x.CustomerId == id)
            .OrderByDescending(x => x.RequestDate)
            .Select(x => new RequestDto(
                x.Id,
                x.CarId,
                x.Car.Make + " " + x.Car.Model,
                x.Car.Company.Name,
                x.Type,
                x.State,
                x.RequestDate,
                x.Deadline,
                x.StartDate,
                x.EndDate))
            .ToListAsync(ct);
    }

    public async Task<OperationResult> CreateRequestAsync(CreateRequestDto request, CancellationToken ct)
    {
        var id = await CustomerId(ct);
        var car = await db.Cars.FirstOrDefaultAsync(x => x.Id == request.CarId && x.DeletedAt == null && x.IsAvailable, ct);

        if (id is null || car is null)
        {
            return OperationResult.Fail("Vehicle is not available.");
        }

        if (request.Type == RequestType.Rent && !DomainRules.IsRentalPeriodValid(request.StartDate, request.EndDate))
        {
            return OperationResult.Fail("A valid rental period is required.");
        }

        if (await db.Requests.AnyAsync(x => x.CustomerId == id && x.CarId == request.CarId && (x.State == RequestState.Pending || x.State == RequestState.Approved), ct))
        {
            return OperationResult.Fail("An active request already exists.");
        }

        db.Requests.Add(new CarRequest
        {
            CustomerId = id.Value,
            CarId = request.CarId,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        });
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> CancelRequestAsync(int id, CancellationToken ct)
    {
        var customerId = await CustomerId(ct);
        var request = await db.Requests.FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);

        if (request is null)
        {
            return OperationResult.Fail("Request not found.");
        }

        if (!DomainRules.CanCancel(request.State))
        {
            return OperationResult.Fail("This request can no longer be cancelled.");
        }

        request.State = RequestState.Cancelled;
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<TransactionDto>> TransactionsAsync(CancellationToken ct)
    {
        var id = await CustomerId(ct);

        return await db.Transactions.AsNoTracking()
            .Where(x => x.Request.CustomerId == id)
            .OrderByDescending(x => x.ListingDate)
            .Select(x => new TransactionDto(
                x.Id,
                x.RequestId,
                x.Request.Car.Make + " " + x.Request.Car.Model,
                x.PaidAmount,
                x.ListingDate,
                x.State,
                x.Rating == null ? null : x.Rating.Value))
            .ToListAsync(ct);
    }

    public async Task<OperationResult> RateAsync(int transactionId, decimal value, CancellationToken ct)
    {
        if (value is < 1 or > 5)
        {
            return OperationResult.Fail("Rating must be between 1 and 5.");
        }

        var customerId = await CustomerId(ct);
        var transaction = await db.Transactions
            .Include(x => x.Rating)
            .Include(x => x.Request)
            .FirstOrDefaultAsync(x => x.Id == transactionId && x.Request.CustomerId == customerId, ct);

        if (transaction is null)
        {
            return OperationResult.Fail("Transaction not found.");
        }

        if (transaction.Rating is null)
        {
            transaction.Rating = new CarRate { CarId = transaction.Request.CarId, Value = value };
        }
        else
        {
            transaction.Rating.Value = value;
        }

        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    private Task<int?> CustomerId(CancellationToken ct)
        => db.Customers.Where(x => x.UserId == current.UserId).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);

    private static OperationResult IdentityResult(IdentityResult result)
        => result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Fail(string.Join(" ", result.Errors.Select(x => x.Description)));
}
