using System.Security.Claims;
using ParkingSystem.Shared.Core.Services;

namespace ParkingSystem.WebApi.Bff.WebApp.Services;

public class HttpContextTenantProvider(IHttpContextAccessor accessor) : ITenantProvider
{
    public long? TenantId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue("tenant_id");
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public bool IsSuperAdmin
        => accessor.HttpContext?.User.IsInRole("SuperAdmin") ?? true;
}
