using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Vaga.Commands;

public class CadastrarVagaCommand(string numero, TipoVaga tipoVaga) : Command
{
    public string Numero { get; } = numero;
    public TipoVaga TipoVaga { get; } = tipoVaga;

    public override bool IsValid()
    {
        ValidationResult = new CadastrarVagaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class CadastrarVagaCommandHandler(IVagaRepository vagaRepository)
    : CommandHandler<CadastrarVagaCommand>
{
    public override async Task<ValidationResult> Handle(CadastrarVagaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var existing = await vagaRepository.GetByNumeroAsync(command.Numero, cancellationToken);
        if (existing is not null)
        {
            AddError($"Já existe uma vaga com o número '{command.Numero}'.");
            return ValidationResult;
        }

        var vaga = new Domain.Entities.Vaga(command.Numero, command.TipoVaga);

        await vagaRepository.AddAsync(vaga, cancellationToken);
        var result = await PersistData(vagaRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = vaga.Id, numero = vaga.Numero };

        return result;
    }
}

public class CadastrarVagaCommandValidator : ValidatorBase<CadastrarVagaCommand>
{
    public CadastrarVagaCommandValidator()
    {
        ValidateNotEmpty(x => x.Numero, "O número da vaga é obrigatório.");
        ValidateMaxLength(x => x.Numero, 10, "O número da vaga deve ter no máximo 10 caracteres.");
        ValidateIsInEnum(x => x.TipoVaga, "Tipo de vaga inválido.");
    }
}
