using System.Linq.Expressions;
using AutoNest.Business.Contracts;
using AutoNest.Data;
using AutoNest.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoNest.Business.Services;

public sealed class CarService(AutoNestDbContext db, IUserContext current) : ICarService
{
    private const string PlaceholderImageUrl = "/api/assets/placeholder.png";

    public async Task<PagedResult<CarSummaryDto>> BrowseAsync(CarFilter filter, CancellationToken ct)
    {
        var query = db.Cars.AsNoTracking().Where(x => x.DeletedAt == null && x.IsAvailable);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(x => (x.Make + " " + x.Model).Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Make))
        {
            query = query.Where(x => x.Make == filter.Make);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(x => x.Type == filter.Type);
        }

        if (filter.GearType.HasValue)
        {
            query = query.Where(x => x.GearType == filter.GearType);
        }

        if (filter.FuelType.HasValue)
        {
            query = query.Where(x => x.FuelType == filter.FuelType);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= filter.MinPrice);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= filter.MaxPrice);
        }

        if (filter.MinYear.HasValue)
        {
            query = query.Where(x => x.Year >= filter.MinYear);
        }

        if (filter.MaxYear.HasValue)
        {
            query = query.Where(x => x.Year <= filter.MaxYear);
        }

        if (filter.IsForSale.HasValue)
        {
            query = query.Where(x => x.IsForSale == filter.IsForSale);
        }

        var page = Math.Max(filter.Page, 1);
        var size = Math.Clamp(filter.PageSize, 1, 50);
        var count = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.ListingDate)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(SummaryProjection)
            .ToListAsync(ct);

        return new(items, page, size, count);
    }

    public async Task<CarDetailsDto?> GetAsync(int id, CancellationToken ct)
    {
        var car = await db.Cars.AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new CarDetailsDto(
                x.Id,
                x.CompanyId,
                x.Company.Name,
                x.Make,
                x.Model,
                x.Year,
                x.Type,
                x.GearType,
                x.FuelType,
                x.SeatsCount,
                x.Mileage,
                x.Price,
                x.IsAvailable,
                x.IsForSale,
                x.InRent,
                x.ListingDate,
                x.Rates.Any() ? x.Rates.Average(r => r.Value) : 0,
                x.Images.Select(i => $"/api/cars/images/{i.Id}").ToList()))
            .FirstOrDefaultAsync(ct);

        return car is not null && car.ImageUrls.Count == 0
            ? car with { ImageUrls = [PlaceholderImageUrl] }
            : car;
    }

    public Task<byte[]?> GetImageAsync(int imageId, CancellationToken ct)
        => db.CarImages.AsNoTracking()
            .Where(x => x.Id == imageId)
            .Select(x => x.Image)
            .FirstOrDefaultAsync(ct);

    public Task<IReadOnlyList<CompanySummaryDto>> CompaniesAsync(CancellationToken ct)
        => db.Companies.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CompanySummaryDto(x.Id, x.Name, x.Email, x.Address.City.Name, x.Address.AreaName))
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<CompanySummaryDto>>(x => x.Result, ct);

    public async Task<PagedResult<CarSummaryDto>> CompanyCarsAsync(int companyId, int page, int pageSize, bool includeDeleted, CancellationToken ct)
    {
        var query = db.Cars.AsNoTracking()
            .Where(x => x.CompanyId == companyId && (includeDeleted ? x.DeletedAt != null : x.DeletedAt == null));
        var p = Math.Max(page, 1);
        var size = Math.Clamp(pageSize, 1, 50);
        var count = await query.CountAsync(ct);

        return new(
            await query.OrderByDescending(x => x.ListingDate)
                .Skip((p - 1) * size)
                .Take(size)
                .Select(SummaryProjection)
                .ToListAsync(ct),
            p,
            size,
            count);
    }

    public async Task<int?> CreateAsync(CarUpsertRequest request, IReadOnlyList<(byte[] Data, string ContentType)> images, CancellationToken ct)
    {
        var companyId = await CurrentCompanyId(ct);

        if (companyId is null || !DomainRules.IsVehicleValid(request))
        {
            return null;
        }

        var car = new Car { CompanyId = companyId.Value };
        Apply(car, request);

        foreach (var image in images)
        {
            car.Images.Add(new CarImage { Image = image.Data, ContentType = image.ContentType });
        }

        db.Cars.Add(car);
        await db.SaveChangesAsync(ct);

        return car.Id;
    }

    public async Task<OperationResult> UpdateAsync(int id, CarUpsertRequest request, IReadOnlyList<(byte[] Data, string ContentType)> images, CancellationToken ct)
    {
        var companyId = await CurrentCompanyId(ct);
        var car = await db.Cars.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);

        if (car is null)
        {
            return OperationResult.Fail("Vehicle not found.");
        }

        if (!DomainRules.IsVehicleValid(request))
        {
            return OperationResult.Fail("Vehicle details are invalid.");
        }

        Apply(car, request);

        if (images.Count > 0)
        {
            db.CarImages.RemoveRange(car.Images);
            car.Images.Clear();

            foreach (var image in images)
            {
                car.Images.Add(new CarImage { Image = image.Data, ContentType = image.ContentType });
            }
        }

        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var car = await Owned(id, ct);

        if (car is null)
        {
            return OperationResult.Fail("Vehicle not found.");
        }

        car.DeletedAt = DateTime.UtcNow;
        car.IsAvailable = false;
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    public async Task<OperationResult> RestoreAsync(int id, CancellationToken ct)
    {
        var car = await Owned(id, ct);

        if (car is null)
        {
            return OperationResult.Fail("Vehicle not found.");
        }

        car.DeletedAt = null;
        car.IsAvailable = true;
        await db.SaveChangesAsync(ct);

        return OperationResult.Success();
    }

    private async Task<Car?> Owned(int id, CancellationToken ct)
    {
        var company = await CurrentCompanyId(ct);

        return await db.Cars.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == company, ct);
    }

    private Task<int?> CurrentCompanyId(CancellationToken ct)
        => db.Companies.Where(x => x.UserId == current.UserId).Select(x => (int?)x.Id).FirstOrDefaultAsync(ct);

    private static void Apply(Car car, CarUpsertRequest x)
    {
        car.Make = x.Make.Trim();
        car.Model = x.Model.Trim();
        car.Year = x.Year;
        car.Type = x.Type;
        car.GearType = x.GearType;
        car.FuelType = x.FuelType;
        car.SeatsCount = x.SeatsCount;
        car.Mileage = x.Mileage;
        car.Price = x.Price;
        car.IsAvailable = x.IsAvailable;
        car.IsForSale = x.IsForSale;
    }

    private static readonly Expression<Func<Car, CarSummaryDto>> SummaryProjection = x => new(
        x.Id,
        x.CompanyId,
        x.Company.Name,
        x.Make,
        x.Model,
        x.Year,
        x.Type,
        x.GearType,
        x.FuelType,
        x.SeatsCount,
        x.Mileage,
        x.Price,
        x.IsAvailable,
        x.IsForSale,
        x.InRent,
        x.Rates.Any() ? x.Rates.Average(r => r.Value) : 0,
        x.Images.Select(i => $"/api/cars/images/{i.Id}").FirstOrDefault() ?? PlaceholderImageUrl);
}
