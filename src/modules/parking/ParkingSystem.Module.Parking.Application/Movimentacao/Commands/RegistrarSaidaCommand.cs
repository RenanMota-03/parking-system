using ParkingSystem.Module.Parking.Application.Tarifa.Services;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Movimentacao.Commands;

public class RegistrarSaidaCommand(string placaVeiculo) : Command
{
    public string PlacaVeiculo { get; } = placaVeiculo;

    public override bool IsValid()
    {
        ValidationResult = new RegistrarSaidaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class RegistrarSaidaCommandHandler(
    IMovimentacaoRepository movimentacaoRepository,
    IVagaRepository vagaRepository,
    ITarifaService tarifaService)
    : CommandHandler<RegistrarSaidaCommand>
{
    public override async Task<ValidationResult> Handle(RegistrarSaidaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var movimentacao = await movimentacaoRepository.GetAbertaByPlacaAsync(command.PlacaVeiculo, cancellationToken);
        if (movimentacao is null)
        {
            AddError($"Nenhuma entrada em aberto encontrada para a placa '{command.PlacaVeiculo}'.");
            return ValidationResult;
        }

        var vaga = await vagaRepository.GetByIdAsync(movimentacao.VagaId, cancellationToken);
        if (vaga is null)
        {
            AddError("Vaga associada à movimentação não encontrada.");
            return ValidationResult;
        }

        var valorTotal = await tarifaService.CalcularAsync(
            vaga.TipoVaga,
            movimentacao.DataEntrada,
            DateTime.UtcNow,
            cancellationToken);

        movimentacao.RegistrarSaida(valorTotal);
        vaga.Liberar();

        movimentacaoRepository.Update(movimentacao);
        vagaRepository.Update(vaga);

        var result = await PersistData(movimentacaoRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = movimentacao.Id, valor_total = movimentacao.ValorTotal, data_saida = movimentacao.DataSaida };

        return result;
    }
}

public class RegistrarSaidaCommandValidator : ValidatorBase<RegistrarSaidaCommand>
{
    public RegistrarSaidaCommandValidator()
    {
        ValidateNotEmpty(x => x.PlacaVeiculo, "A placa do veículo é obrigatória.");
        ValidateMaxLength(x => x.PlacaVeiculo, 8, "A placa deve ter no máximo 8 caracteres.");
    }
}
