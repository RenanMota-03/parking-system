using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Application.Movimentacao.Commands;

public class PagarCommand(long movimentacaoId, FormaPagamento formaPagamento) : Command
{
    public long MovimentacaoId { get; } = movimentacaoId;
    public FormaPagamento FormaPagamento { get; } = formaPagamento;

    public override bool IsValid()
    {
        ValidationResult = new PagarCommandValidator().Validate(this);
        return ValidationResult.IsValid;
    }
}

internal class PagarCommandHandler(IMovimentacaoRepository movimentacaoRepository)
    : CommandHandler<PagarCommand>
{
    public override async Task<ValidationResult> Handle(PagarCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.IsValid()) return command.ValidationResult!;

        var movimentacao = await movimentacaoRepository.GetByIdAsync(command.MovimentacaoId, cancellationToken);
        if (movimentacao is null)
        {
            AddError($"Movimentação com id '{command.MovimentacaoId}' não encontrada.");
            return ValidationResult;
        }

        movimentacao.Pagar(command.FormaPagamento);
        movimentacaoRepository.Update(movimentacao);

        return await PersistData(movimentacaoRepository.UnitOfWork);
    }
}

public class PagarCommandValidator : ValidatorBase<PagarCommand>
{
    public PagarCommandValidator()
    {
        ValidateMust(x => x.MovimentacaoId > 0, nameof(PagarCommand.MovimentacaoId), "O id da movimentação é obrigatório.");
        ValidateIsInEnum(x => x.FormaPagamento, "Forma de pagamento inválida.");
    }
}
