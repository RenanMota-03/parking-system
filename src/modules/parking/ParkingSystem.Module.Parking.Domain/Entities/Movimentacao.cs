using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Domain.Entities;

public class Movimentacao : TrackableEntity, IAggregateRoot, ITenantEntity
{
    public long TenantId { get; private set; }
    public long VagaId { get; private set; }
    public string PlacaVeiculo { get; private set; } = string.Empty;
    public DateTime DataEntrada { get; private set; }
    public DateTime? DataSaida { get; private set; }
    public decimal? ValorTotal { get; private set; }
    public bool Pago { get; private set; }
    public FormaPagamento? FormaPagamento { get; private set; }

    public Vaga? Vaga { get; private set; }

    protected Movimentacao() { }

    public Movimentacao(long tenantId, long vagaId, string placaVeiculo)
    {
        TenantId = tenantId;
        VagaId = vagaId;
        PlacaVeiculo = placaVeiculo.ToUpperInvariant();
        DataEntrada = DateTime.UtcNow;
        Pago = false;
        SetCreatedNow();
        Validate();
    }

    public void RegistrarSaida(decimal valorTotal)
    {
        if (DataSaida is not null)
            throw new InvalidOperationException("A saída já foi registrada para esta movimentação.");

        DataSaida = DateTime.UtcNow;
        ValorTotal = valorTotal;
        SetUpdatedNow();
    }

    public void Pagar(FormaPagamento formaPagamento)
    {
        if (DataSaida is null)
            throw new InvalidOperationException("Não é possível pagar uma movimentação sem saída registrada.");
        if (Pago)
            throw new InvalidOperationException("Esta movimentação já foi paga.");

        Pago = true;
        FormaPagamento = formaPagamento;
        SetUpdatedNow();
    }

    public bool EstaAberta() => DataSaida is null;

    private void Validate()
    {
        DomainValidation.NotNullOrEmpty(PlacaVeiculo, nameof(PlacaVeiculo));
        DomainValidation.MaxLength(PlacaVeiculo, 8, nameof(PlacaVeiculo));
        DomainValidation.NotZeroOrNegative(VagaId, nameof(VagaId));
    }
}
