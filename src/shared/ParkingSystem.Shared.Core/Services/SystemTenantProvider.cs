namespace ParkingSystem.Shared.Core.Services;

public class SystemTenantProvider : ITenantProvider
{
    public long? TenantId => null;
    public bool IsSuperAdmin => true;
}
