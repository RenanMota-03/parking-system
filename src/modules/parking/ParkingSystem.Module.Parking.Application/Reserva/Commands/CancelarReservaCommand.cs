using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Reserva.Commands;

public class CancelarReservaCommand(long reservaId) : Command
{
    public long ReservaId { get; } = reservaId;

    public override bool IsValid()
    {
        ValidationResult = new CancelarReservaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class CancelarReservaCommandHandler(
    IReservaRepository reservaRepository)
    : CommandHandler<CancelarReservaCommand>
{
    public override async Task<ValidationResult> Handle(CancelarReservaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var reserva = await reservaRepository.GetByIdAsync(command.ReservaId, cancellationToken);
        if (reserva is null)
        {
            AddError($"Reserva com id '{command.ReservaId}' não encontrada.");
            return ValidationResult;
        }

        if (!reserva.EstaAtiva())
        {
            AddError("Apenas reservas ativas podem ser canceladas.");
            return ValidationResult;
        }

        reserva.Cancelar();
        reserva.Vaga?.LiberarReserva();

        reservaRepository.Update(reserva);

        return await PersistData(reservaRepository.UnitOfWork);
    }
}

public class CancelarReservaCommandValidator : ValidatorBase<CancelarReservaCommand>
{
    public CancelarReservaCommandValidator()
    {
        ValidateMust(x => x.ReservaId > 0, nameof(CancelarReservaCommand.ReservaId), "O id da reserva é obrigatório.");
    }
}
