using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Infra.Data.EF;

namespace ParkingSystem.Workers.ReservaExpiracao;

public class ReservaExpiracaoWorker(
    ILogger<ReservaExpiracaoWorker> logger,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalo = TimeSpan.FromMinutes(
            configuration.GetValue<int>("ReservaExpiracao:IntervaloEmMinutos", 5));

        logger.LogInformation("Worker iniciado. Intervalo: {Intervalo} minutos.", intervalo.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ExpirarReservasVencidasAsync(stoppingToken);
            await Task.Delay(intervalo, stoppingToken);
        }
    }

    private async Task ExpirarReservasVencidasAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ParkingDbContext>();

        var agora = DateTime.UtcNow;

        var vencidas = await context.Reservas
            .Include(r => r.Vaga)
            .Where(r => (r.Status == StatusReserva.Pendente || r.Status == StatusReserva.Confirmada)
                        && r.DataLimite < agora)
            .ToListAsync(ct);

        if (vencidas.Count == 0)
            return;

        foreach (var reserva in vencidas)
        {
            reserva.Expirar();
            reserva.Vaga?.LiberarReserva();
        }

        await context.Commit();

        logger.LogInformation("{Count} reserva(s) expirada(s) em {Agora:u}.", vencidas.Count, agora);
    }
}
