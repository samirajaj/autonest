using Microsoft.AspNetCore.Mvc.Testing;
using AutoNest.Data;
using AutoNest.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace AutoNest.Api.Tests;

public sealed class ApiSmokeTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Public_car_catalog_is_available()
    {
        var response = await factory.CreateClient().GetAsync("/api/cars");

        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task Public_city_lookup_is_available()
    {
        var response = await factory.CreateClient().GetAsync("/api/cities");

        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    [Fact]
    public async Task Profile_requires_authentication()
    {
        var response = await factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }).GetAsync("/api/profile");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Placeholder_image_is_served_as_a_shared_static_asset()
    {
        var response = await factory.CreateClient().GetAsync("/api/assets/placeholder.png");

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.True((await response.Content.ReadAsByteArrayAsync()).Length > 0);
    }

    [Fact]
    public async Task Car_image_uses_its_stored_content_type()
    {
        int imageId;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AutoNestDbContext>();
            var image = new CarImage { CarId = int.MaxValue, Image = [1, 2, 3], ContentType = "image/png" };
            db.CarImages.Add(image);
            await db.SaveChangesAsync();
            imageId = image.Id;
        }

        var response = await factory.CreateClient().GetAsync($"/api/cars/images/{imageId}");

        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
    }
}
