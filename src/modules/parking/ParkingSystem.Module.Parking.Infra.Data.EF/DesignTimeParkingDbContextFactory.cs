using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ParkingSystem.Shared.Core.Services;

namespace ParkingSystem.Module.Parking.Infra.Data.EF;

public class DesignTimeParkingDbContextFactory : IDesignTimeDbContextFactory<ParkingDbContext>
{
    public ParkingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ParkingDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SecretPassword123!";
        optionsBuilder.UseNpgsql(connectionString);

        return new ParkingDbContext(optionsBuilder.Options, new SystemTenantProvider());
    }
}
