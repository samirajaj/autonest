using AutoNest.Business.Contracts;
using AutoNest.Data;
using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Business.Services;

public sealed class ManagementService(
    AutoNestDbContext db,
    IUserContext current,
    UserManager<ApplicationUser> users) : IManagementService
{
    public async Task<DashboardDto> DashboardAsync(bool admin, int year, CancellationToken ct)
    {
        var companyId = admin ? null : await CurrentCompanyId(ct);
        var cars = db.Cars.Where(x => admin || x.CompanyId == companyId);
        var requests = db.Requests.Where(x => admin || x.Car.CompanyId == companyId);
        var transactions = db.Transactions.Where(x => admin || x.Request.Car.CompanyId == companyId);
        var metrics = new List<DashboardMetricDto>
        {
            new("Vehicles", await cars.CountAsync(ct)),
            new("Available vehicles", await cars.CountAsync(x => x.DeletedAt == null && x.IsAvailable, ct)),
            new("Requests", await requests.CountAsync(ct)),
            new("Active rentals", await requests.CountAsync(x => x.Type == RequestType.Rent && x.State == RequestState.Approved, ct)),
            new("Transactions", await transactions.CountAsync(ct)),
            new("Earnings", await transactions.SumAsync(x => (decimal?)x.PaidAmount, ct) ?? 0, "currency")
        };

        if (admin)
        {
            metrics.Add(new("Companies", await db.Companies.CountAsync(ct)));
            metrics.Add(new("Customers", await db.Customers.CountAsync(ct)));
        }

        var quarters = new decimal[4];
        var amounts = await transactions
            .Where(x => x.ListingDate.Year == year)
            .Select(x => new { x.ListingDate.Month, x.PaidAmount })
            .ToListAsync(ct);

        foreach (var item in amounts)
        {
            quarters[DomainRules.Quarter(new DateTime(year, item.Month, 1))] += item.PaidAmount;
        }

        return new(metrics, quarters);
    }

    public async Task<PagedResult<RequestDto>> CompanyRequestsAsync(RequestState? state, int page, int size, CancellationToken ct)
    {
        var companyId = await CurrentCompanyId(ct);
        var query = db.Requests.AsNoTracking().Where(x => x.Car.CompanyId == companyId);

        if (state.HasValue)
        {
            query = query.Where(x => x.State == state);
        }

        var p = Math.Max(1, page);
        var s = Math.Clamp(size, 1, 50);
        var count = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.RequestDate)
            .Skip((p - 1) * s)
            .Take(s)
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

        return new(items, p, s, count);
    }

    public async Task<OperationResult> ApproveAsync(int id, ApproveRequestDto input, CancellationToken ct)
    {
        var companyId = await CurrentCompanyId(ct);
        var request = await db.Requests.Include(x => x.Car).FirstOrDefaultAsync(x => x.Id == id && x.Car.CompanyId == companyId, ct);

        if (request is null)
        {
            return OperationResult.Fail("Request not found.");
        }

        if (!DomainRules.CanApprove(request.State))
        {
            return OperationResult.Fail("Only pending requests can be approved.");
        }

        request.State = RequestState.Approved;
        request.Deadline = input.Deadline;
        request.Car.IsAvailable = false;
        request.Car.InRent = request.Type == RequestType.Rent;

        if (request.Type == RequestType.Sale)
        {
            request.Car.SoldAt = DateTime.UtcNow;
        }

        db.Transactions.Add(new Transaction
        {
            Request = request,
            PaidAmount = input.PaidAmount,
            State = request.Type == RequestType.Sale ? TransactionStatus.Sold : TransactionStatus.Rented
        });
        await AddPoints(request.CustomerId, input.PaidAmount, ct);
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> RejectAsync(int id, CancellationToken ct)
    {
        var companyId = await CurrentCompanyId(ct);
        var request = await db.Requests.FirstOrDefaultAsync(x => x.Id == id && x.Car.CompanyId == companyId, ct);

        if (request is null)
        {
            return OperationResult.Fail("Request not found.");
        }

        if (request.State != RequestState.Pending)
        {
            return OperationResult.Fail("Only pending requests can be rejected.");
        }

        request.State = RequestState.Rejected;
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<PagedResult<AdminCustomerDto>> CustomersAsync(int page, int size, CancellationToken ct)
    {
        var p = Math.Max(page, 1);
        var s = Math.Clamp(size, 1, 50);
        var q = db.Customers.AsNoTracking();

        return new(
            await q.OrderBy(x => x.FirstName)
                .Skip((p - 1) * s)
                .Take(s)
                .Select(x => new AdminCustomerDto(x.Id, x.UserId, x.FirstName + " " + x.LastName, x.User.Email!, x.User.LockoutEnd > DateTimeOffset.UtcNow, x.Point))
                .ToListAsync(ct),
            p,
            s,
            await q.CountAsync(ct));
    }

    public async Task<PagedResult<AdminCompanyDto>> CompaniesAsync(int page, int size, CancellationToken ct)
    {
        var p = Math.Max(page, 1);
        var s = Math.Clamp(size, 1, 50);
        var q = db.Companies.AsNoTracking();

        return new(
            await q.OrderBy(x => x.Name)
                .Skip((p - 1) * s)
                .Take(s)
                .Select(x => new AdminCompanyDto(x.Id, x.UserId, x.Name, x.Email, x.User.LockoutEnd > DateTimeOffset.UtcNow, x.SubscriptionPlan == null ? null : x.SubscriptionPlan.PlanId))
                .ToListAsync(ct),
            p,
            s,
            await q.CountAsync(ct));
    }

    public async Task<OperationResult> SetLockAsync(string userId, bool locked)
    {
        var user = await users.FindByIdAsync(userId);

        if (user is null)
        {
            return OperationResult.Fail("Account not found.");
        }

        user.LockoutEnd = locked ? DateTimeOffset.MaxValue : null;

        return Identity(await users.UpdateAsync(user));
    }

    public async Task<OperationResult> CreateCustomerAsync(AdminCustomerUpsertRequest x, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(x.Password))
        {
            return OperationResult.Fail("A password is required.");
        }

        var user = new ApplicationUser { Email = x.Email, UserName = x.UserName, EmailConfirmed = true };
        var made = await users.CreateAsync(user, x.Password);

        if (!made.Succeeded)
        {
            return Identity(made);
        }

        await users.AddToRoleAsync(user, "Customer");

        db.Customers.Add(new Customer
        {
            UserId = user.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            BirthDate = x.BirthDate,
            Address = new Address { CityId = x.CityId, AreaName = x.AreaName }
        });
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> CreateCompanyAsync(AdminCompanyUpsertRequest x, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(x.Password))
        {
            return OperationResult.Fail("A password is required.");
        }

        var user = new ApplicationUser { Email = x.Email, UserName = x.UserName, EmailConfirmed = true };
        var made = await users.CreateAsync(user, x.Password);

        if (!made.Succeeded)
        {
            return Identity(made);
        }

        await users.AddToRoleAsync(user, "Company");

        var company = new Company
        {
            UserId = user.Id,
            Name = x.Name,
            Email = x.Email,
            Address = new Address { CityId = x.CityId, AreaName = x.AreaName }
        };

        foreach (var contact in x.Contacts)
        {
            company.Contacts.Add(new Contact { Type = contact.Type, Value = contact.Value });
        }

        db.Companies.Add(company);
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> UpdateCustomerAsync(int id, AdminCustomerUpsertRequest x, CancellationToken ct)
    {
        var customer = await db.Customers.Include(c => c.User).Include(c => c.Address).FirstOrDefaultAsync(c => c.Id == id, ct);

        if (customer is null)
        {
            return OperationResult.Fail("Customer not found.");
        }

        customer.FirstName = x.FirstName;
        customer.LastName = x.LastName;
        customer.BirthDate = x.BirthDate;
        customer.Address.CityId = x.CityId;
        customer.Address.AreaName = x.AreaName;
        customer.User.Email = x.Email;
        customer.User.UserName = x.UserName;
        customer.User.NormalizedEmail = users.NormalizeEmail(x.Email);
        customer.User.NormalizedUserName = users.NormalizeName(x.UserName);
        customer.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(x.Password))
        {
            return await ChangePasswordAsync(customer.UserId, x.Password);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> UpdateCompanyAsync(int id, AdminCompanyUpsertRequest x, CancellationToken ct)
    {
        var company = await db.Companies
            .Include(c => c.User)
            .Include(c => c.Address)
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (company is null)
        {
            return OperationResult.Fail("Company not found.");
        }

        company.Name = x.Name;
        company.Email = x.Email;
        company.Address.CityId = x.CityId;
        company.Address.AreaName = x.AreaName;
        company.User.Email = x.Email;
        company.User.UserName = x.UserName;
        company.User.NormalizedEmail = users.NormalizeEmail(x.Email);
        company.User.NormalizedUserName = users.NormalizeName(x.UserName);
        company.UpdatedAt = DateTime.UtcNow;
        db.Contacts.RemoveRange(company.Contacts);
        company.Contacts.Clear();

        foreach (var contact in x.Contacts)
        {
            company.Contacts.Add(new Contact { Type = contact.Type, Value = contact.Value });
        }

        await db.SaveChangesAsync(ct);

        if (!string.IsNullOrWhiteSpace(x.Password))
        {
            return await ChangePasswordAsync(company.UserId, x.Password);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> ChangePasswordAsync(string userId, string password)
    {
        var user = await users.FindByIdAsync(userId);

        if (user is null)
        {
            return OperationResult.Fail("Account not found.");
        }

        var token = await users.GeneratePasswordResetTokenAsync(user);

        return Identity(await users.ResetPasswordAsync(user, token, password));
    }

    public async Task<IReadOnlyList<PlanDto>> PlansAsync(CancellationToken ct)
        => await db.Plans.AsNoTracking().OrderBy(x => x.Price).Select(x => new PlanDto(x.Id, x.Name, x.Duration, x.Price)).ToListAsync(ct);

    public async Task<OperationResult> SavePlanAsync(int? id, PlanUpsertRequest x, CancellationToken ct)
    {
        if (x.Duration <= 0 || x.Price < 0 || string.IsNullOrWhiteSpace(x.Name))
        {
            return OperationResult.Fail("Plan details are invalid.");
        }

        var plan = id.HasValue ? await db.Plans.FindAsync([id.Value], ct) : new Plan();

        if (plan is null)
        {
            return OperationResult.Fail("Plan not found.");
        }

        plan.Name = x.Name.Trim();
        plan.Duration = x.Duration;
        plan.Price = x.Price;

        if (!id.HasValue)
        {
            db.Plans.Add(plan);
        }

        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeletePlanAsync(int id, CancellationToken ct)
    {
        var plan = await db.Plans.FindAsync([id], ct);

        if (plan is null)
        {
            return OperationResult.Fail("Plan not found.");
        }

        if (await db.SubscriptionPlans.AnyAsync(x => x.PlanId == id, ct))
        {
            return OperationResult.Fail("Assigned plans cannot be deleted.");
        }

        db.Plans.Remove(plan);
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> AssignPlanAsync(int companyId, int planId, CancellationToken ct)
    {
        var company = await db.Companies.FindAsync([companyId], ct);
        var plan = await db.Plans.FindAsync([planId], ct);

        if (company is null || plan is null)
        {
            return OperationResult.Fail("Company or plan not found.");
        }

        var subscription = new SubscriptionPlan
        {
            CompanyId = companyId,
            PlanId = planId,
            PaidAmount = plan.Price,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(plan.Duration)
        };
        db.SubscriptionPlans.Add(subscription);
        await db.SaveChangesAsync(ct);
        company.SubscriptionPlanId = subscription.Id;
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<IReadOnlyList<PointRangeDto>> PointRangesAsync(CancellationToken ct)
        => await db.PointRanges.AsNoTracking().OrderBy(x => x.MinAmount).Select(x => new PointRangeDto(x.Id, x.MinAmount, x.MaxAmount, x.Point)).ToListAsync(ct);

    public async Task<OperationResult> SavePointRangeAsync(int? id, PointRangeUpsertRequest x, CancellationToken ct)
    {
        if (x.MinAmount < 0 || x.MaxAmount < x.MinAmount || x.Point < 0)
        {
            return OperationResult.Fail("Point range is invalid.");
        }

        if (await db.PointRanges.AnyAsync(r => r.Id != id && r.MinAmount <= x.MaxAmount && r.MaxAmount >= x.MinAmount, ct))
        {
            return OperationResult.Fail("Point ranges cannot overlap.");
        }

        var range = id.HasValue ? await db.PointRanges.FindAsync([id.Value], ct) : new PointRange();

        if (range is null)
        {
            return OperationResult.Fail("Point range not found.");
        }

        range.MinAmount = x.MinAmount;
        range.MaxAmount = x.MaxAmount;
        range.Point = x.Point;

        if (!id.HasValue)
        {
            db.PointRanges.Add(range);
        }

        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeletePointRangeAsync(int id, CancellationToken ct)
    {
        var range = await db.PointRanges.FindAsync([id], ct);

        if (range is null)
        {
            return OperationResult.Fail("Point range not found.");
        }

        db.PointRanges.Remove(range);
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    private async Task AddPoints(int customerId, decimal amount, CancellationToken ct)
    {
        var customer = await db.Customers.FindAsync([customerId], ct);

        if (customer is null)
        {
            return;
        }

        var range = await db.PointRanges.FirstOrDefaultAsync(x => amount >= x.MinAmount && amount <= x.MaxAmount, ct);

        if (range is not null)
        {
            customer.Point += range.Point;
        }
    }

    private Task<int?> CurrentCompanyId(CancellationToken ct)
        => db.Companies.Where(x => x.UserId == current.UserId).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);

    private static OperationResult Identity(IdentityResult x)
        => x.Succeeded
            ? OperationResult.Success()
            : OperationResult.Fail(string.Join(" ", x.Errors.Select(e => e.Description)));
}
