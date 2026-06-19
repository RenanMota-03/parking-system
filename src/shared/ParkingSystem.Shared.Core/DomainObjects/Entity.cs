using ParkingSystem.Shared.Core.Messages;

namespace ParkingSystem.Shared.Core.DomainObjects;

public abstract class Entity
{
    public long Id { get; set; }

    private readonly List<Event> _events = [];
    public IReadOnlyCollection<Event> Notifications => _events.AsReadOnly();

    public void AddEvent(Event @event) => _events.Add(@event);
    public void RemoveEvent(Event @event) => _events.Remove(@event);
    public void ClearEvents() => _events.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (GetType() != obj.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => GetType().GetHashCode() ^ Id.GetHashCode();
    public override string ToString() => $"{GetType().Name} [Id={Id}]";

    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}
