using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ParkingSystem.Module.Identity.Infra.Data.EF;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using Xunit;

namespace ParkingSystem.Tests.IntegrationTests;

public class ParkingWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestDb = "parking_system_integration_test_db";
    private const string TestJwtSecret = "integration-test-secret-key-minimum-32-chars!";

    private static readonly string TestConnectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?.Replace("parking_system_db", TestDb)
        ?? $"Host=localhost;Port=5432;Database={TestDb};Username=parking_admin;Password=SecretPassword123!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = TestConnectionString,
                ["Jwt:Secret"] = TestJwtSecret,
            });
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var identityCtx = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var parkingCtx = scope.ServiceProvider.GetRequiredService<ParkingDbContext>();

        await identityCtx.Database.EnsureCreatedAsync();
        await parkingCtx.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var identityCtx = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var parkingCtx = scope.ServiceProvider.GetRequiredService<ParkingDbContext>();

        await identityCtx.Database.EnsureDeletedAsync();
        await parkingCtx.Database.EnsureDeletedAsync();

        await base.DisposeAsync();
    }
}
