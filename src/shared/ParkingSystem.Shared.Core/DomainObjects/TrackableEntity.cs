namespace ParkingSystem.Shared.Core.DomainObjects;

public abstract class TrackableEntity : Entity, ITrackableEntity
{
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected void SetCreatedNow()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    protected void SetUpdatedNow()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void Delete()
    {
        IsDeleted = true;
        SetUpdatedNow();
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        SetUpdatedNow();
    }
}
