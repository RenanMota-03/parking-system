using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Domain.Entities;

public class Reserva : TrackableEntity, IAggregateRoot, ITenantEntity
{
    public long TenantId { get; private set; }
    public long VagaId { get; private set; }
    public string UsuarioId { get; private set; } = string.Empty;
    public DateTime DataAgendada { get; private set; }
    public DateTime DataLimite { get; private set; }
    public StatusReserva Status { get; private set; }

    public Vaga? Vaga { get; private set; }

    protected Reserva() { }

    public Reserva(long tenantId, long vagaId, string usuarioId, DateTime dataAgendada, DateTime dataLimite)
    {
        DomainValidation.NotNullOrEmpty(usuarioId, nameof(UsuarioId));
        DomainValidation.That(dataLimite > dataAgendada, "DataLimite deve ser posterior à DataAgendada.");
        DomainValidation.That(dataAgendada > DateTime.UtcNow, "DataAgendada deve ser uma data futura.");

        TenantId = tenantId;
        VagaId = vagaId;
        UsuarioId = usuarioId;
        DataAgendada = dataAgendada;
        DataLimite = dataLimite;
        Status = StatusReserva.Pendente;
        SetCreatedNow();
    }

    public void Confirmar()
    {
        if (Status != StatusReserva.Pendente)
            throw new InvalidOperationException("Apenas reservas pendentes podem ser confirmadas.");
        Status = StatusReserva.Confirmada;
        SetUpdatedNow();
    }

    public void Cancelar()
    {
        if (Status is StatusReserva.Utilizada or StatusReserva.Expirada)
            throw new InvalidOperationException("Reserva já encerrada não pode ser cancelada.");
        Status = StatusReserva.Cancelada;
        SetUpdatedNow();
    }

    public void Expirar()
    {
        if (Status != StatusReserva.Pendente && Status != StatusReserva.Confirmada)
            throw new InvalidOperationException("Reserva não pode ser expirada no estado atual.");
        Status = StatusReserva.Expirada;
        SetUpdatedNow();
    }

    public void Utilizar()
    {
        if (Status != StatusReserva.Confirmada && Status != StatusReserva.Pendente)
            throw new InvalidOperationException("Apenas reservas ativas podem ser utilizadas.");
        Status = StatusReserva.Utilizada;
        SetUpdatedNow();
    }

    public bool EstaAtiva() => Status is StatusReserva.Pendente or StatusReserva.Confirmada;
}
