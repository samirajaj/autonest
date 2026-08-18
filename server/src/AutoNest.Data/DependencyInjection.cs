using AutoNest.Data.Entities;
using AutoNest.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoNest.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AutoNestDbContext>(options => options.UseSqlServer(connectionString));
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        }).AddRoles<IdentityRole>().AddEntityFrameworkStores<AutoNestDbContext>().AddDefaultTokenProviders();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
