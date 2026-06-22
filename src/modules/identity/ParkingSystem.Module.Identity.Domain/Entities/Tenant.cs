using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Identity.Domain.Entities;

public class Tenant : TrackableEntity, IAggregateRoot
{
    public string Nome { get; private set; } = string.Empty;
    public string CodigoConvite { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }

    protected Tenant() { }

    public Tenant(string nome, string codigoConvite)
    {
        DomainValidation.NotNullOrEmpty(nome, nameof(Nome));
        DomainValidation.MaxLength(nome, 150, nameof(Nome));
        DomainValidation.NotNullOrEmpty(codigoConvite, nameof(CodigoConvite));

        Nome = nome;
        CodigoConvite = codigoConvite;
        Ativo = true;
        SetCreatedNow();
    }
}
