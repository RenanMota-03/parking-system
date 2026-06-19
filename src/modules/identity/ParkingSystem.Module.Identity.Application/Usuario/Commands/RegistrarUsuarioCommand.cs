using Microsoft.AspNetCore.Identity;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Module.Identity.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Identity.Application.Usuario.Commands;

public class RegistrarUsuarioCommand(string nome, string email, string senha, Role role) : Command
{
    public string Nome { get; } = nome;
    public string Email { get; } = email;
    public string Senha { get; } = senha;
    public Role Role { get; } = role;

    public override bool IsValid()
    {
        ValidationResult = new RegistrarUsuarioCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class RegistrarUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    : CommandHandler<RegistrarUsuarioCommand>
{
    private readonly PasswordHasher<Domain.Entities.Usuario> _hasher = new();

    public override async Task<ValidationResult> Handle(RegistrarUsuarioCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var existe = await usuarioRepository.ExisteEmailAsync(command.Email, cancellationToken);
        if (existe)
        {
            AddError("Já existe um usuário cadastrado com este e-mail.");
            return ValidationResult;
        }

        var hash = _hasher.HashPassword(null!, command.Senha);
        var usuario = new Domain.Entities.Usuario(command.Nome, command.Email, hash, command.Role);

        await usuarioRepository.AddAsync(usuario, cancellationToken);

        var result = await PersistData(usuarioRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = usuario.Id, email = usuario.Email, role = usuario.Role.ToString() };

        return result;
    }
}

public class RegistrarUsuarioCommandValidator : ValidatorBase<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioCommandValidator()
    {
        ValidateNotEmpty(x => x.Nome, "O nome é obrigatório.");
        ValidateMaxLength(x => x.Nome, 100, "O nome deve ter no máximo 100 caracteres.");
        ValidateNotEmpty(x => x.Email, "O e-mail é obrigatório.");
        ValidateMaxLength(x => x.Email, 150, "O e-mail deve ter no máximo 150 caracteres.");
        ValidateNotEmpty(x => x.Senha, "A senha é obrigatória.");
        ValidateMust(x => x.Senha.Length >= 6, nameof(RegistrarUsuarioCommand.Senha), "A senha deve ter no mínimo 6 caracteres.");
    }
}
