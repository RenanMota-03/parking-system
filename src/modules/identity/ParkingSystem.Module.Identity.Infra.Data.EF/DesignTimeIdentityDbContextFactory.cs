using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ParkingSystem.Module.Identity.Infra.Data.EF;

public class DesignTimeIdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=parking_system_db;Username=parking_admin;Password=SecretPassword123!";

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new IdentityDbContext(options);
    }
}
