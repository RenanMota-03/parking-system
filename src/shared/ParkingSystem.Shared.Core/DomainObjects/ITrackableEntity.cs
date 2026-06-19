namespace ParkingSystem.Shared.Core.DomainObjects;

public interface ITrackableEntity
{
    bool IsDeleted { get; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
    void Delete();
    void Restore();
}
