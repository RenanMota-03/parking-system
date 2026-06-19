using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Workers.ReservaExpiracao;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddDbContext<ParkingDbContext>(options =>
            options.UseNpgsql(ctx.Configuration.GetConnectionString("DefaultConnection")));

        services.AddHostedService<ReservaExpiracaoWorker>();
    })
    .Build();

await host.RunAsync();
