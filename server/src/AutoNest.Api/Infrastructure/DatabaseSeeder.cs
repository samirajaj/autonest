using AutoNest.Data;
using AutoNest.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Api.Infrastructure;

public static class DatabaseSeeder
{
    private static readonly (string Make, string Model)[] Fleet =
    [
        ("Toyota", "Corolla"),
        ("Toyota", "Camry"),
        ("Hyundai", "Elantra"),
        ("Hyundai", "Tucson"),
        ("Honda", "Civic"),
        ("Honda", "CR-V"),
        ("Kia", "Sportage"),
        ("Kia", "Cerato"),
        ("Nissan", "Sunny"),
        ("Nissan", "X-Trail"),
        ("Ford", "Focus"),
        ("Ford", "Escape"),
        ("Chevrolet", "Cruze"),
        ("Chevrolet", "Trailblazer"),
        ("Volkswagen", "Golf"),
        ("Volkswagen", "Tiguan"),
        ("BMW", "320i"),
        ("Mercedes-Benz", "C200"),
        ("Audi", "A3"),
        ("Geely", "Coolray")
    ];

    private static readonly string[] SyrianGovernorates =
    [
        "Damascus",
        "Aleppo",
        "Homs",
        "Latakia",
        "Hama",
        "Tartus",
        "Idlib",
        "Raqqa",
        "Deir ez-Zor",
        "Hasakah",
        "Daraa",
        "Suwayda",
        "Rif Dimashq",
        "Quneitra"
    ];

    private static readonly string[] Areas =
    [
        "Downtown",
        "Al-Mazzeh",
        "Kafar Souseh",
        "Al-Hamra",
        "City Center",
        "Al-Ramel"
    ];

    private static readonly (string First, string Last)[] CustomerNames =
    [
        ("Ahmad", "Hassan"),
        ("Lina", "Khalil"),
        ("Omar", "Nasser"),
        ("Rana", "Saleh"),
        ("Khaled", "Aziz"),
        ("Nour", "Hamdan"),
        ("Fadi", "Mansour"),
        ("Dima", "Rahal"),
        ("Samer", "Jaber"),
        ("Hiba", "Sawaya")
    ];

    private static readonly (string Name, string Email, int City)[] CompanySeed =
    [
        ("Damascus Motors", "damascus.motors@autonest.com", 0),
        ("Aleppo Autos", "aleppo.autos@autonest.com", 1),
        ("Homs Car Center", "homs.carcenter@autonest.com", 2),
        ("Latakia Wheels", "latakia.wheels@autonest.com", 3),
        ("Coastal Cars", "coastal.cars@autonest.com", 3)
    ];


    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var db = services.GetRequiredService<AutoNestDbContext>();
        await db.Database.MigrateAsync();

        await EnsureRolesAsync(services);
        await EnsureCitiesAsync(db);
        await EnsurePlansAsync(db);
        await EnsurePointRangesAsync(db);
        await EnsureAdminAsync(services, config);

        if (await db.Companies.AnyAsync() || await db.Customers.AnyAsync())
        {
            return;
        }

        await SeedMockDataAsync(services, db, config);
    }

    private static async Task EnsureRolesAsync(IServiceProvider services)
    {
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Customer", "Company", "Admin" })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task EnsureCitiesAsync(AutoNestDbContext db)
    {
        var existing = (await db.Cities.AsNoTracking().Select(x => x.Name).ToListAsync()).ToHashSet();
        var missing = SyrianGovernorates
            .Where(name => !existing.Contains(name))
            .Select(name => new City { Name = name })
            .ToList();

        if (missing.Count > 0)
        {
            db.Cities.AddRange(missing);
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsurePlansAsync(AutoNestDbContext db)
    {
        if (await db.Plans.AnyAsync())
        {
            return;
        }

        db.Plans.AddRange(
            new Plan { Name = "Basic", Duration = 30, Price = 150m },
            new Plan { Name = "Standard", Duration = 90, Price = 400m },
            new Plan { Name = "Premium", Duration = 365, Price = 1400m });
        await db.SaveChangesAsync();
    }

    private static async Task EnsurePointRangesAsync(AutoNestDbContext db)
    {
        if (await db.PointRanges.AnyAsync())
        {
            return;
        }

        db.PointRanges.AddRange(
            new PointRange { MinAmount = 0, MaxAmount = 999, Point = 1 },
            new PointRange { MinAmount = 1000, MaxAmount = 4999, Point = 5 },
            new PointRange { MinAmount = 5000, MaxAmount = 9999, Point = 10 },
            new PointRange { MinAmount = 10000, MaxAmount = 9999999, Point = 25 });
        await db.SaveChangesAsync();
    }

    private static async Task EnsureAdminAsync(IServiceProvider services, IConfiguration config)
    {
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var email = RequiredSetting(config, "Admin:Email");
        var userName = RequiredSetting(config, "Admin:UserName");
        var password = RequiredSetting(config, "Admin:Password");

        if (await users.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };
        var result = await users.CreateAsync(admin, password);

        if (result.Succeeded)
        {
            await users.AddToRoleAsync(admin, "Admin");
        }
    }

    private static string RequiredSetting(IConfiguration config, string key)
    {
        var value = config[key];

        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException(
                $"{key} is required. Configure it with user-secrets or an environment variable.");
    }

    private static async Task SeedMockDataAsync(
        IServiceProvider services,
        AutoNestDbContext db,
        IConfiguration config)
    {
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var cities = await db.Cities.OrderBy(x => x.Id).ToListAsync();
        var plans = await db.Plans.OrderBy(x => x.Id).ToListAsync();
        var defaultPassword = RequiredSetting(config, "Seed:DefaultPassword");

        var companies = new List<Company>();

        for (var i = 0; i < CompanySeed.Length; i++)
        {
            var seed = CompanySeed[i];
            var user = await CreateUserAsync(users, seed.Email, "Company", defaultPassword);

            var company = new Company
            {
                UserId = user.Id,
                Name = seed.Name,
                Email = seed.Email,
                Address = new Address
                {
                    CityId = cities[seed.City].Id,
                    AreaName = Areas[i % Areas.Length]
                },
                Contacts =
                [
                    new Contact { Type = "Phone", Value = $"0{i + 1}{i + 2} 555 1{i}{i} {i}{i}" },
                    new Contact { Type = "WhatsApp", Value = $"+963 9{i} {i}{i} {i}{i}{i}{i}" },
                    new Contact { Type = "Website", Value = $"www.{(seed.Name.Replace(" ", "-")).ToLowerInvariant()}.com" }
                ],
                UpdatedAt = DateTime.UtcNow
            };

            companies.Add(company);
            db.Companies.Add(company);
        }

        await db.SaveChangesAsync();

        for (var i = 0; i < companies.Count; i++)
        {
            var company = companies[i];
            var plan = plans[i % plans.Count];
            var subscription = new SubscriptionPlan
            {
                CompanyId = company.Id,
                PlanId = plan.Id,
                PaidAmount = plan.Price,
                StartDate = DateTime.UtcNow.AddDays(-60),
                EndDate = DateTime.UtcNow.AddDays(plan.Duration - 60)
            };
            db.SubscriptionPlans.Add(subscription);
            await db.SaveChangesAsync();

            company.SubscriptionPlanId = subscription.Id;
        }

        await db.SaveChangesAsync();

        var customers = new List<Customer>();

        for (var i = 0; i < CustomerNames.Length; i++)
        {
            var (first, last) = CustomerNames[i];
            var email = $"customer{i + 1}@autonest.com";
            var user = await CreateUserAsync(users, email, "Customer", defaultPassword);

            var customer = new Customer
            {
                UserId = user.Id,
                FirstName = first,
                LastName = last,
                BirthDate = new DateOnly(1986 + (i % 15), (i % 6) + 1, (i % 20) + 1),
                Address = new Address
                {
                    CityId = cities[i % cities.Count].Id,
                    AreaName = Areas[(i + 2) % Areas.Length]
                },
                Point = 0,
                UpdatedAt = DateTime.UtcNow
            };

            customers.Add(customer);
            db.Customers.Add(customer);
        }

        await db.SaveChangesAsync();

        for (var i = 0; i < companies.Count; i++)
        {
            var company = companies[i];
            var rng = new Random(500 + i);

            for (var j = 0; j < 12; j++)
            {
                var car = BuildCar(company.Id, i, j, rng);
                db.Cars.Add(car);
            }
        }

        await db.SaveChangesAsync();

        var carsByCompany = db.Cars.AsNoTracking().ToLookup(x => x.CompanyId);

        foreach (var company in companies)
        {
            var ids = carsByCompany[company.Id].Select(x => x.Id).OrderBy(x => x).ToList();
            var deleted = (await db.Cars.FindAsync(ids[^1]))!;
            deleted.DeletedAt = DateTime.UtcNow.AddDays(-(30 + company.Id));
            deleted.IsAvailable = false;
        }

        await db.SaveChangesAsync();

        await SeedActivityAsync(db, customers);
    }

    private static async Task SeedActivityAsync(AutoNestDbContext db, List<Customer> customers)
    {
        var cars = await db.Cars.OrderBy(x => x.Id).ToListAsync();
        var pool = cars.Where(x => x.DeletedAt == null).ToList();
        var now = DateTime.UtcNow;

        for (var i = 0; i < customers.Count; i++)
        {
            var customer = customers[i];
            var rng = new Random(1000 + i);
            var start = i * 5;

            var saleCar = pool[start];
            var activeCar = pool[start + 1];
            var rentCar = pool[start + 2];
            var pendingCar = pool[start + 3];
            var cancelledCar = pool[start + 4];

            saleCar.IsAvailable = false;
            saleCar.IsForSale = true;
            saleCar.SoldAt = now.AddDays(-rng.Next(30, 300));
            saleCar.InRent = false;

            activeCar.IsAvailable = false;
            activeCar.InRent = true;

            rentCar.IsAvailable = true;
            rentCar.InRent = false;

            var saleDate = RandomDate(rng, new DateTime(2025, 9, 1), new DateTime(2026, 6, 30));
            var rentDate = RandomDate(rng, new DateTime(2025, 9, 1), new DateTime(2026, 6, 30));

            var saleRequest = new CarRequest
            {
                CustomerId = customer.Id,
                CarId = saleCar.Id,
                State = RequestState.Completed,
                Type = RequestType.Sale,
                RequestDate = saleDate.AddDays(-10),
                Deadline = saleDate
            };
            db.Requests.Add(saleRequest);
            customer.Point += PointsFor(saleCar.Price);
            db.Transactions.Add(CreateTransaction(saleRequest, saleCar, saleCar.Price, saleDate, TransactionStatus.Sold, rng.Next(30, 51) / 10m));

            var activeRequest = new CarRequest
            {
                CustomerId = customer.Id,
                CarId = activeCar.Id,
                State = RequestState.Approved,
                Type = RequestType.Rent,
                RequestDate = now.AddDays(-rng.Next(5, 20)),
                Deadline = now.AddDays(rng.Next(15, 40)),
                StartDate = now.AddDays(-rng.Next(5, 20)),
                EndDate = now.AddDays(rng.Next(15, 40))
            };
            db.Requests.Add(activeRequest);
            var activeAmount = Math.Round(activeCar.Price * 30, 2);
            customer.Point += PointsFor(activeAmount);
            db.Transactions.Add(CreateTransaction(activeRequest, activeCar, activeAmount, activeRequest.RequestDate, TransactionStatus.Rented, null));

            var pastStart = rentDate.AddDays(-14);
            var pastEnd = rentDate;
            var rentRequest = new CarRequest
            {
                CustomerId = customer.Id,
                CarId = rentCar.Id,
                State = RequestState.Completed,
                Type = RequestType.Rent,
                RequestDate = pastStart.AddDays(-2),
                Deadline = pastEnd,
                StartDate = pastStart,
                EndDate = pastEnd
            };
            db.Requests.Add(rentRequest);
            var rentAmount = Math.Round(rentCar.Price * 14, 2);
            customer.Point += PointsFor(rentAmount);
            db.Transactions.Add(CreateTransaction(rentRequest, rentCar, rentAmount, rentDate, TransactionStatus.Rented, rng.Next(35, 51) / 10m));

            db.Requests.Add(new CarRequest
            {
                CustomerId = customer.Id,
                CarId = pendingCar.Id,
                State = RequestState.Pending,
                Type = i % 2 == 0 ? RequestType.Rent : RequestType.Sale,
                RequestDate = now.AddDays(-rng.Next(1, 4)),
                StartDate = i % 2 == 0 ? now.AddDays(3) : null,
                EndDate = i % 2 == 0 ? now.AddDays(12) : null
            });

            db.Requests.Add(new CarRequest
            {
                CustomerId = customer.Id,
                CarId = cancelledCar.Id,
                State = RequestState.Cancelled,
                Type = i % 2 == 0 ? RequestType.Sale : RequestType.Rent,
                RequestDate = now.AddDays(-rng.Next(20, 40))
            });

            var used = new HashSet<int> { start, start + 1, start + 2, start + 3, start + 4 };

            for (var k = 0; used.Count < 9; k++)
            {
                used.Add((i * 11 + k * 13) % pool.Count);
            }

            foreach (var index in used)
            {
                db.FavoriteCars.Add(new FavoriteCar { CustomerId = customer.Id, CarId = pool[index].Id });
            }
        }

        await db.SaveChangesAsync();
    }

    private static Transaction CreateTransaction(CarRequest request, Car car, decimal amount, DateTime listingDate, TransactionStatus state, decimal? rating)
    {
        var transaction = new Transaction
        {
            Request = request,
            PaidAmount = amount,
            ListingDate = listingDate,
            State = state
        };

        if (rating.HasValue)
        {
            transaction.Rating = new CarRate { CarId = car.Id, Value = rating.Value };
        }

        return transaction;
    }

    private static int PointsFor(decimal amount)
        => amount switch
        {
            < 1000 => 1,
            < 5000 => 5,
            < 10000 => 10,
            _ => 25
        };

    private static Car BuildCar(int companyId, int companyIndex, int index, Random rng)
    {
        var (make, model) = Fleet[(companyIndex * 2 + index) % Fleet.Length];
        var forSale = rng.Next(0, 3) == 0;

        return new Car
        {
            CompanyId = companyId,
            Make = make,
            Model = model,
            Year = rng.Next(2016, 2025),
            Type = (CarType)rng.Next(0, 7),
            GearType = (GearType)rng.Next(0, 4),
            FuelType = (FuelType)rng.Next(0, 5),
            SeatsCount = rng.Next(2, 9),
            Mileage = rng.Next(8000, 160000),
            Price = forSale ? rng.Next(12000, 85000) : rng.Next(35, 180),
            IsAvailable = true,
            IsForSale = forSale,
            InRent = false,
            ListingDate = RandomDate(rng, new DateTime(2025, 1, 1), new DateTime(2026, 7, 31))
        };
    }

    private static DateTime RandomDate(Random rng, DateTime from, DateTime to)
        => from.AddDays(rng.Next(0, (int)(to - from).TotalDays));

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> users,
        string email,
        string role,
        string password)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };
        var result = await users.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        await users.AddToRoleAsync(user, role);

        return user;
    }
}
