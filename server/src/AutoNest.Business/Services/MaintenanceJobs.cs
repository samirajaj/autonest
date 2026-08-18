using AutoNest.Data;
using AutoNest.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Business.Services;

public sealed class MaintenanceJobs(AutoNestDbContext db, AutoNest.Business.Contracts.IEmailService email)
{
    public async Task DeleteOldCarsAsync()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);
        var cars = await db.Cars.Where(x => x.DeletedAt < cutoff).ToListAsync();
        db.Cars.RemoveRange(cars);
        await db.SaveChangesAsync();
    }

    public async Task ExpireRequestsAsync()
    {
        var now = DateTime.UtcNow;
        var pending = await db.Requests.Where(x => x.State == RequestState.Pending && x.RequestDate < now.AddDays(-7)).ToListAsync();

        foreach (var request in pending)
        {
            request.State = RequestState.Obsoleted;
        }

        var approved = await db.Requests.Include(x => x.Car).Where(x => x.State == RequestState.Approved && x.Deadline < now).ToListAsync();

        foreach (var request in approved)
        {
            request.State = RequestState.Rejected;
            request.Car.IsAvailable = true;
            request.Car.InRent = false;
        }

        await db.SaveChangesAsync();
    }

    public async Task ExpirePlansAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await db.Companies
            .Include(x => x.SubscriptionPlan)
            .Where(x => x.SubscriptionPlan != null && x.SubscriptionPlan.EndDate < now)
            .ToListAsync();

        foreach (var company in expired)
        {
            company.SubscriptionPlanId = null;
        }

        await db.SaveChangesAsync();
    }

    public async Task NotifyPlansAsync()
    {
        var deadline = DateTime.UtcNow.AddDays(3);
        var subscriptions = await db.SubscriptionPlans
            .Include(x => x.Company)
            .Where(x => x.EndDate >= DateTime.UtcNow && x.EndDate <= deadline)
            .ToListAsync();

        foreach (var item in subscriptions)
        {
            await email.SendAsync(item.Company.Email, "AutoNest plan expiry", $"Your plan expires on {item.EndDate:d}.");
        }
    }

    public async Task NotifyRentalsAsync()
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var rentals = await db.Requests
            .Include(x => x.Customer).ThenInclude(x => x.User)
            .Include(x => x.Car)
            .Where(x => x.Type == RequestType.Rent && x.State == RequestState.Approved && x.EndDate != null && x.EndDate.Value.Date == tomorrow)
            .ToListAsync();

        foreach (var item in rentals)
        {
            await email.SendAsync(item.Customer.User.Email!, "AutoNest rental reminder", $"Your {item.Car.Make} {item.Car.Model} rental ends tomorrow.");
        }
    }
}
