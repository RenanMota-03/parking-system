using ParkingSystem.Shared.Core.Data;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Shared.Core.Messages;

public abstract class CommandHandler
{
    protected readonly ValidationResult ValidationResult = new();

    protected void AddError(string message)
        => ValidationResult.AddFailure("Error", message);

    protected async Task<ValidationResult> PersistData(IUnitOfWork uow)
    {
        if (!await uow.Commit())
            AddError("Ocorreu um erro ao persistir os dados.");
        return ValidationResult;
    }
}

public abstract class CommandHandler<TCommand> : CommandHandler, ICommandHandler<TCommand, ValidationResult>
    where TCommand : Command
{
    public abstract Task<ValidationResult> Handle(TCommand command, CancellationToken cancellationToken = default);
}
