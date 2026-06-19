using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Shared.Core.Messages;

public abstract class Command : Message
{
    public DateTime Timestamp { get; private set; } = DateTime.Now;
    public ValidationResult? ValidationResult { get; protected set; }
    public virtual bool IsValid() { throw new NotImplementedException(); }
}
