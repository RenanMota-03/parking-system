using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Movimentacao.Commands;

public class RegistrarEntradaCommand(long vagaId, string placaVeiculo) : Command
{
    public long VagaId { get; } = vagaId;
    public string PlacaVeiculo { get; } = placaVeiculo;

    public override bool IsValid()
    {
        ValidationResult = new RegistrarEntradaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class RegistrarEntradaCommandHandler(
    IVagaRepository vagaRepository,
    IMovimentacaoRepository movimentacaoRepository)
    : CommandHandler<RegistrarEntradaCommand>
{
    public override async Task<ValidationResult> Handle(RegistrarEntradaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var vaga = await vagaRepository.GetByIdAsync(command.VagaId, cancellationToken);
        if (vaga is null)
        {
            AddError($"Vaga com id '{command.VagaId}' não encontrada.");
            return ValidationResult;
        }

        var aberta = await movimentacaoRepository.GetAbertaByPlacaAsync(command.PlacaVeiculo, cancellationToken);
        if (aberta is not null)
        {
            AddError($"O veículo '{command.PlacaVeiculo}' já possui uma entrada em aberto.");
            return ValidationResult;
        }

        vaga.Ocupar();
        vagaRepository.Update(vaga);

        var movimentacao = new Domain.Entities.Movimentacao(vaga.TenantId, command.VagaId, command.PlacaVeiculo);
        await movimentacaoRepository.AddAsync(movimentacao, cancellationToken);

        var result = await PersistData(movimentacaoRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = movimentacao.Id, placa = movimentacao.PlacaVeiculo, data_entrada = movimentacao.DataEntrada };

        return result;
    }
}

public class RegistrarEntradaCommandValidator : ValidatorBase<RegistrarEntradaCommand>
{
    public RegistrarEntradaCommandValidator()
    {
        ValidateMust(x => x.VagaId > 0, nameof(RegistrarEntradaCommand.VagaId), "O id da vaga é obrigatório.");
        ValidateNotEmpty(x => x.PlacaVeiculo, "A placa do veículo é obrigatória.");
        ValidateMaxLength(x => x.PlacaVeiculo, 8, "A placa deve ter no máximo 8 caracteres.");
    }
}
