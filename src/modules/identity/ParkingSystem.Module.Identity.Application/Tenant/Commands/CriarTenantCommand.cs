using ParkingSystem.Module.Identity.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Identity.Application.Tenant.Commands;

public class CriarTenantCommand(string nome) : Command
{
    public string Nome { get; } = nome;

    public override bool IsValid()
    {
        ValidationResult = new CriarTenantCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class CriarTenantCommandHandler(ITenantRepository tenantRepository)
    : CommandHandler<CriarTenantCommand>
{
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public override async Task<ValidationResult> Handle(CriarTenantCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var codigo = GerarCodigo();
        var tenant = new Domain.Entities.Tenant(command.Nome, codigo);

        await tenantRepository.AddAsync(tenant, cancellationToken);

        var result = await PersistData(tenantRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = tenant.Id, nome = tenant.Nome, codigoConvite = tenant.CodigoConvite };

        return result;
    }

    private static string GerarCodigo()
    {
        var chars = new char[8];
        for (var i = 0; i < chars.Length; i++)
            chars[i] = Chars[Random.Shared.Next(Chars.Length)];
        return new string(chars);
    }
}

public class CriarTenantCommandValidator : ValidatorBase<CriarTenantCommand>
{
    public CriarTenantCommandValidator()
    {
        ValidateNotEmpty(x => x.Nome, "O nome do tenant é obrigatório.");
        ValidateMaxLength(x => x.Nome, 150, "O nome deve ter no máximo 150 caracteres.");
    }
}
