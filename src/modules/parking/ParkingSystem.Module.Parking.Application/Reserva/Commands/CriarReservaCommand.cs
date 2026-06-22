using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Reserva.Commands;

public class CriarReservaCommand(long vagaId, string usuarioId, DateTime dataAgendada, DateTime dataLimite) : Command
{
    public long VagaId { get; } = vagaId;
    public string UsuarioId { get; } = usuarioId;
    public DateTime DataAgendada { get; } = dataAgendada;
    public DateTime DataLimite { get; } = dataLimite;

    public override bool IsValid()
    {
        ValidationResult = new CriarReservaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class CriarReservaCommandHandler(
    IVagaRepository vagaRepository,
    IReservaRepository reservaRepository)
    : CommandHandler<CriarReservaCommand>
{
    public override async Task<ValidationResult> Handle(CriarReservaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var vaga = await vagaRepository.GetByIdAsync(command.VagaId, cancellationToken);
        if (vaga is null)
        {
            AddError($"Vaga com id '{command.VagaId}' não encontrada.");
            return ValidationResult;
        }

        var reservasAtivas = await reservaRepository.GetAtivasByVagaIdAsync(command.VagaId, cancellationToken);
        var conflito = reservasAtivas.Any(r =>
            command.DataAgendada < r.DataLimite && command.DataLimite > r.DataAgendada);

        if (conflito)
        {
            AddError("Já existe uma reserva ativa para esta vaga no período solicitado.");
            return ValidationResult;
        }

        vaga.Reservar();
        vagaRepository.Update(vaga);

        var reserva = new Domain.Entities.Reserva(vaga.TenantId, command.VagaId, command.UsuarioId, command.DataAgendada, command.DataLimite);
        await reservaRepository.AddAsync(reserva, cancellationToken);

        var result = await PersistData(reservaRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = reserva.Id };

        return result;
    }
}

public class CriarReservaCommandValidator : ValidatorBase<CriarReservaCommand>
{
    public CriarReservaCommandValidator()
    {
        ValidateMust(x => x.VagaId > 0, nameof(CriarReservaCommand.VagaId), "O id da vaga é obrigatório.");
        ValidateNotEmpty(x => x.UsuarioId, "O id do usuário é obrigatório.");
        ValidateMust(x => x.DataAgendada > DateTime.UtcNow, nameof(CriarReservaCommand.DataAgendada), "A data agendada deve ser futura.");
        ValidateMust(x => x.DataLimite > x.DataAgendada, nameof(CriarReservaCommand.DataLimite), "A data limite deve ser posterior à data agendada.");
    }
}
