using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Shared.Core.Services;
using ParkingSystem.Workers.ReservaExpiracao;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddSingleton<ITenantProvider, SystemTenantProvider>();
        services.AddDbContext<ParkingDbContext>(options =>
            options.UseNpgsql(ctx.Configuration.GetConnectionString("DefaultConnection")));

        services.AddHostedService<ReservaExpiracaoWorker>();
    })
    .Build();

await host.RunAsync();
