using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Data;

public sealed class AutoNestDbContext(DbContextOptions<AutoNestDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<City> Cities => Set<City>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<CarImage> CarImages => Set<CarImage>();
    public DbSet<FavoriteCar> FavoriteCars => Set<FavoriteCar>();
    public DbSet<CarRequest> Requests => Set<CarRequest>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<CarRate> CarRates => Set<CarRate>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<PointRange> PointRanges => Set<PointRange>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("dbo");
        builder.Entity<ApplicationUser>().ToTable("Users", "Security");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roles", "Security");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles", "Security");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims", "Security");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins", "Security");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims", "Security");
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens", "Security");

        builder.Entity<Customer>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<Company>().HasIndex(x => x.UserId).IsUnique();
        builder.Entity<Plan>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<FavoriteCar>().HasIndex(x => new { x.CustomerId, x.CarId }).IsUnique();
        builder.Entity<Transaction>().HasIndex(x => x.RequestId).IsUnique();
        builder.Entity<CarRate>().HasIndex(x => x.TransactionId).IsUnique();

        builder.Entity<Car>().Property(x => x.Price).HasPrecision(18, 2);
        builder.Entity<CarRate>().Property(x => x.Value).HasPrecision(3, 2);
        builder.Entity<Plan>().Property(x => x.Price).HasPrecision(18, 2);
        builder.Entity<SubscriptionPlan>().Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Entity<Transaction>().Property(x => x.PaidAmount).HasPrecision(18, 2);

        builder.Entity<Car>().HasMany(x => x.Images).WithOne(x => x.Car).HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Company>().HasOne(x => x.SubscriptionPlan).WithOne(x => x.Company).HasForeignKey<Company>(x => x.SubscriptionPlanId).OnDelete(DeleteBehavior.SetNull);

        foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()))
        {
            if (relationship.DeleteBehavior == DeleteBehavior.Cascade && relationship.DeclaringEntityType.ClrType != typeof(CarImage))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
