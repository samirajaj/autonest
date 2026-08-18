using AutoNest.Business.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoNest.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICarService, CarService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IManagementService, ManagementService>();
        services.AddScoped<MaintenanceJobs>();
        return services;
    }
}
