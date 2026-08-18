using Microsoft.AspNetCore.Mvc.Testing;

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
        Assert.True((await response.Content.ReadAsByteArrayAsync()).Length > 0);
    }
}
