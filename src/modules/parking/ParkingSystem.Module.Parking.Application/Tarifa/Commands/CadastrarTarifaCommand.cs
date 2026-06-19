using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Tarifa.Commands;

public class CadastrarTarifaCommand(
    TipoVaga tipoVaga,
    decimal valorHora,
    decimal valorDiaria,
    decimal valorMensal,
    DateTime? vigenteAte = null) : Command
{
    public TipoVaga TipoVaga { get; } = tipoVaga;
    public decimal ValorHora { get; } = valorHora;
    public decimal ValorDiaria { get; } = valorDiaria;
    public decimal ValorMensal { get; } = valorMensal;
    public DateTime? VigenteAte { get; } = vigenteAte;

    public override bool IsValid()
    {
        ValidationResult = new CadastrarTarifaCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class CadastrarTarifaCommandHandler(ITarifaRepository tarifaRepository)
    : CommandHandler<CadastrarTarifaCommand>
{
    public override async Task<ValidationResult> Handle(CadastrarTarifaCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var tarifa = new Domain.Entities.Tarifa(
            command.TipoVaga,
            command.ValorHora,
            command.ValorDiaria,
            command.ValorMensal,
            command.VigenteAte);

        await tarifaRepository.AddAsync(tarifa, cancellationToken);
        var result = await PersistData(tarifaRepository.UnitOfWork);

        if (result.IsValid)
            result.Data = new { id = tarifa.Id, tipo_vaga = tarifa.TipoVaga };

        return result;
    }
}

public class CadastrarTarifaCommandValidator : ValidatorBase<CadastrarTarifaCommand>
{
    public CadastrarTarifaCommandValidator()
    {
        ValidateIsInEnum(x => x.TipoVaga, "Tipo de vaga inválido.");
        ValidateMust(x => x.ValorHora > 0, nameof(CadastrarTarifaCommand.ValorHora), "O valor da hora deve ser maior que zero.");
        ValidateMust(x => x.ValorDiaria > 0, nameof(CadastrarTarifaCommand.ValorDiaria), "O valor da diária deve ser maior que zero.");
        ValidateMust(x => x.ValorMensal > 0, nameof(CadastrarTarifaCommand.ValorMensal), "O valor mensal deve ser maior que zero.");
    }
}
