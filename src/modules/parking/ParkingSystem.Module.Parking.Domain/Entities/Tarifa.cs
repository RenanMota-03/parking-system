using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Domain.Entities;

public class Tarifa : TrackableEntity, IAggregateRoot
{
    public TipoVaga TipoVaga { get; private set; }
    public decimal ValorHora { get; private set; }
    public decimal ValorDiaria { get; private set; }
    public decimal ValorMensal { get; private set; }
    public DateTime? VigenteAte { get; private set; }

    protected Tarifa() { }

    public Tarifa(TipoVaga tipoVaga, decimal valorHora, decimal valorDiaria, decimal valorMensal, DateTime? vigenteAte = null)
    {
        TipoVaga = tipoVaga;
        ValorHora = valorHora;
        ValorDiaria = valorDiaria;
        ValorMensal = valorMensal;
        VigenteAte = vigenteAte;
        SetCreatedNow();
        Validate();
    }

    public void Atualizar(decimal valorHora, decimal valorDiaria, decimal valorMensal, DateTime? vigenteAte = null)
    {
        ValorHora = valorHora;
        ValorDiaria = valorDiaria;
        ValorMensal = valorMensal;
        VigenteAte = vigenteAte;
        SetUpdatedNow();
        Validate();
    }

    public bool EstaVigente()
        => VigenteAte is null || VigenteAte >= DateTime.UtcNow;

    private void Validate()
    {
        DomainValidation.PositiveDecimal(ValorHora, nameof(ValorHora));
        DomainValidation.PositiveDecimal(ValorDiaria, nameof(ValorDiaria));
        DomainValidation.PositiveDecimal(ValorMensal, nameof(ValorMensal));
    }
}
