using System.Text;
using System.Text.Json.Serialization;
using AutoNest.Api.Infrastructure;
using AutoNest.Business;
using AutoNest.Business.Contracts;
using AutoNest.Business.Services;
using AutoNest.Data;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x => x.AddSecurityDefinition("Bearer", new()
{
    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
}));

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
}

var connectionString = builder.Configuration.GetConnectionString("AutoNest");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:AutoNest is required. Configure it with user-secrets or an environment variable.");
}

builder.Services.AddData(connectionString);
builder.Services.AddBusiness();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    using (var installConnection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
    {
        Hangfire.SqlServer.SqlServerObjectsInstaller.Install(installConnection);
    }

    builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        PrepareSchemaIfNecessary = true
    }));
    builder.Services.AddHangfireServer();
}

var key = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key must contain at least 32 characters. Configure it with user-secrets or an environment variable.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x => x.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        NameClaimType = System.Security.Claims.ClaimTypes.Name,
        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(x => x.AddPolicy("Client", p => p
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles(new StaticFileOptions { RequestPath = "/api/assets" });
app.UseCors("Client");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new AdminDashboardFilter()]
    });

    using var scope = app.Services.CreateScope();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider, builder.Configuration);

    RecurringJob.AddOrUpdate<MaintenanceJobs>("car-cleanup", x => x.DeleteOldCarsAsync(), Cron.Daily);
    RecurringJob.AddOrUpdate<MaintenanceJobs>("request-expiry", x => x.ExpireRequestsAsync(), Cron.Hourly);
    RecurringJob.AddOrUpdate<MaintenanceJobs>("plan-expiry", x => x.ExpirePlansAsync(), Cron.Daily);
    RecurringJob.AddOrUpdate<MaintenanceJobs>("plan-notifications", x => x.NotifyPlansAsync(), Cron.Daily);
    RecurringJob.AddOrUpdate<MaintenanceJobs>("rental-notifications", x => x.NotifyRentalsAsync(), Cron.Daily);
}

app.Run();

public partial class Program;
