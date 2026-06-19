using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Vaga.Commands;

public class AlterarStatusVagaCommand(long vagaId, StatusVaga novoStatus) : Command
{
    public long VagaId { get; } = vagaId;
    public StatusVaga NovoStatus { get; } = novoStatus;

    public override bool IsValid()
    {
        ValidationResult = new AlterarStatusVagaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class AlterarStatusVagaCommandHandler(IVagaRepository vagaRepository)
    : CommandHandler<AlterarStatusVagaCommand>
{
    public override async Task<ValidationResult> Handle(AlterarStatusVagaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var vaga = await vagaRepository.GetByIdAsync(command.VagaId, cancellationToken);
        if (vaga is null)
        {
            AddError($"Vaga com id '{command.VagaId}' não encontrada.");
            return ValidationResult;
        }

        switch (command.NovoStatus)
        {
            case StatusVaga.Manutencao:
                vaga.BloquearManutencao();
                break;
            case StatusVaga.Disponivel:
                vaga.DesbloquearManutencao();
                break;
            default:
                AddError("Operação inválida. Use este endpoint apenas para bloquear ou desbloquear manutenção.");
                return ValidationResult;
        }

        vagaRepository.Update(vaga);
        return await PersistData(vagaRepository.UnitOfWork);
    }
}

public class AlterarStatusVagaCommandValidator : ValidatorBase<AlterarStatusVagaCommand>
{
    public AlterarStatusVagaCommandValidator()
    {
        ValidateMust(x => x.VagaId > 0, nameof(AlterarStatusVagaCommand.VagaId), "O id da vaga é obrigatório.");
        ValidateIsInEnum(x => x.NovoStatus, "Status inválido.");
    }
}
