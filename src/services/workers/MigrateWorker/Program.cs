using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ParkingSystem.Module.Identity.Infra.Data.EF;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Shared.Core.Services;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("ERRO: ConnectionStrings__DefaultConnection não configurada.");
    return 1;
}

Console.WriteLine("Aplicando migrations...");

try
{
    var systemProvider = new SystemTenantProvider();

    var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
        .UseNpgsql(connectionString)
        .Options;

    await using (var identityCtx = new IdentityDbContext(identityOptions, systemProvider))
    {
        Console.WriteLine("[Identity] Aplicando...");
        await identityCtx.Database.MigrateAsync();
        Console.WriteLine("[Identity] OK.");
    }

    var parkingOptions = new DbContextOptionsBuilder<ParkingDbContext>()
        .UseNpgsql(connectionString)
        .Options;

    await using (var parkingCtx = new ParkingDbContext(parkingOptions, systemProvider))
    {
        Console.WriteLine("[Parking] Aplicando...");
        await parkingCtx.Database.MigrateAsync();
        Console.WriteLine("[Parking] OK.");
    }

    Console.WriteLine("Migrations concluídas.");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"ERRO ao aplicar migrations: {ex.Message}");
    return 1;
}
