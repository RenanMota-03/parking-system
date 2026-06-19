using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Tarifa.Commands;

public class AtualizarTarifaCommand(
    long tarifaId,
    decimal valorHora,
    decimal valorDiaria,
    decimal valorMensal,
    DateTime? vigenteAte = null) : Command
{
    public long TarifaId { get; } = tarifaId;
    public decimal ValorHora { get; } = valorHora;
    public decimal ValorDiaria { get; } = valorDiaria;
    public decimal ValorMensal { get; } = valorMensal;
    public DateTime? VigenteAte { get; } = vigenteAte;

    public override bool IsValid()
    {
        ValidationResult = new AtualizarTarifaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class AtualizarTarifaCommandHandler(ITarifaRepository tarifaRepository)
    : CommandHandler<AtualizarTarifaCommand>
{
    public override async Task<ValidationResult> Handle(AtualizarTarifaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var tarifa = await tarifaRepository.GetByIdAsync(command.TarifaId, cancellationToken);
        if (tarifa is null)
        {
            AddError($"Tarifa com id '{command.TarifaId}' não encontrada.");
            return ValidationResult;
        }

        tarifa.Atualizar(command.ValorHora, command.ValorDiaria, command.ValorMensal, command.VigenteAte);

        tarifaRepository.Update(tarifa);
        return await PersistData(tarifaRepository.UnitOfWork);
    }
}

public class AtualizarTarifaCommandValidator : ValidatorBase<AtualizarTarifaCommand>
{
    public AtualizarTarifaCommandValidator()
    {
        ValidateMust(x => x.TarifaId > 0, nameof(AtualizarTarifaCommand.TarifaId), "O id da tarifa é obrigatório.");
        ValidateMust(x => x.ValorHora > 0, nameof(AtualizarTarifaCommand.ValorHora), "O valor da hora deve ser maior que zero.");
        ValidateMust(x => x.ValorDiaria > 0, nameof(AtualizarTarifaCommand.ValorDiaria), "O valor da diária deve ser maior que zero.");
        ValidateMust(x => x.ValorMensal > 0, nameof(AtualizarTarifaCommand.ValorMensal), "O valor mensal deve ser maior que zero.");
    }
}
