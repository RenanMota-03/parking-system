using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Identity.Domain.Entities;

public class Usuario : TrackableEntity, IAggregateRoot
{
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public Role Role { get; private set; }

    protected Usuario() { }

    public Usuario(string nome, string email, string senhaHash, Role role)
    {
        DomainValidation.NotNullOrEmpty(nome, nameof(Nome));
        DomainValidation.MaxLength(nome, 100, nameof(Nome));
        DomainValidation.NotNullOrEmpty(email, nameof(Email));
        DomainValidation.MaxLength(email, 150, nameof(Email));
        DomainValidation.NotNullOrEmpty(senhaHash, nameof(SenhaHash));

        Nome = nome;
        Email = email.ToLowerInvariant();
        SenhaHash = senhaHash;
        Role = role;
        SetCreatedNow();
    }

    public void AtualizarSenha(string novoHash)
    {
        DomainValidation.NotNullOrEmpty(novoHash, nameof(SenhaHash));
        SenhaHash = novoHash;
        SetUpdatedNow();
    }
}
