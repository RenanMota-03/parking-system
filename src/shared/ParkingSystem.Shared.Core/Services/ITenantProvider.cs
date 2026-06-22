namespace ParkingSystem.Shared.Core.Services;

public interface ITenantProvider
{
    long? TenantId { get; }
    bool IsSuperAdmin { get; }
}
