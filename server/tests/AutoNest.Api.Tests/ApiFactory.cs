using AutoNest.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AutoNest.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(x =>
        {
            x.ClearProviders();
            x.AddConsole();
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AutoNestDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AutoNestDbContext>>();
            services.AddDbContext<AutoNestDbContext>(x => x.UseInMemoryDatabase("autonest-tests"));
        });
    }
}
