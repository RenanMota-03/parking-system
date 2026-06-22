using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Domain.Entities;

public class Vaga : TrackableEntity, IAggregateRoot, ITenantEntity
{
    public long TenantId { get; private set; }
    public string Numero { get; private set; } = string.Empty;
    public TipoVaga TipoVaga { get; private set; }
    public StatusVaga Status { get; private set; }

    protected Vaga() { }

    public Vaga(long tenantId, string numero, TipoVaga tipoVaga)
    {
        TenantId = tenantId;
        Numero = numero;
        TipoVaga = tipoVaga;
        Status = StatusVaga.Disponivel;
        SetCreatedNow();
        Validate();
    }

    public void Ocupar()
    {
        if (Status != StatusVaga.Disponivel)
            throw new InvalidOperationException($"Vaga {Numero} não está disponível para ocupação.");
        Status = StatusVaga.Ocupada;
        SetUpdatedNow();
    }

    public void Liberar()
    {
        if (Status != StatusVaga.Ocupada)
            throw new InvalidOperationException($"Vaga {Numero} não está ocupada.");
        Status = StatusVaga.Disponivel;
        SetUpdatedNow();
    }

    public void Reservar()
    {
        if (Status != StatusVaga.Disponivel)
            throw new InvalidOperationException($"Vaga {Numero} não está disponível para reserva.");
        Status = StatusVaga.Reservada;
        SetUpdatedNow();
    }

    public void LiberarReserva()
    {
        if (Status != StatusVaga.Reservada)
            throw new InvalidOperationException($"Vaga {Numero} não está reservada.");
        Status = StatusVaga.Disponivel;
        SetUpdatedNow();
    }

    public void BloquearManutencao()
    {
        Status = StatusVaga.Manutencao;
        SetUpdatedNow();
    }

    public void DesbloquearManutencao()
    {
        if (Status != StatusVaga.Manutencao)
            throw new InvalidOperationException($"Vaga {Numero} não está em manutenção.");
        Status = StatusVaga.Disponivel;
        SetUpdatedNow();
    }

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(Numero, nameof(Numero));
        DomainValidation.MaxLength(Numero, 10, nameof(Numero));
    }
}
