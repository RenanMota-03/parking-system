using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;

namespace ParkingSystem.Module.Parking.Application.Tarifa.Services;

public interface ITarifaService
{
    Task<decimal> CalcularAsync(TipoVaga tipoVaga, DateTime entrada, DateTime saida, CancellationToken ct = default);
}

public class TarifaService(ITarifaRepository tarifaRepository) : ITarifaService
{
    private const double MinutosCarencia = 15;

    public async Task<decimal> CalcularAsync(TipoVaga tipoVaga, DateTime entrada, DateTime saida, CancellationToken ct = default)
    {
        var duracao = saida - entrada;

        if (duracao.TotalMinutes < MinutosCarencia)
            return 0m;

        var tarifa = await tarifaRepository.GetVigenteByTipoVagaAsync(tipoVaga, ct)
            ?? throw new InvalidOperationException($"Nenhuma tarifa vigente encontrada para o tipo de vaga '{tipoVaga}'.");

        var horas = (decimal)Math.Ceiling(duracao.TotalHours);
        var valor = horas * tarifa.ValorHora;

        return valor > tarifa.ValorDiaria ? tarifa.ValorDiaria : valor;
    }
}
