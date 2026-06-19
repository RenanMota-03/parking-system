using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Shared.Core.Exceptions;

public class EntityValidationException : Exception
{
    public IReadOnlyCollection<ValidationError>? Errors { get; }

    public EntityValidationException(string? message, IReadOnlyCollection<ValidationError>? errors = null)
        : base(message)
        => Errors = errors;
}
