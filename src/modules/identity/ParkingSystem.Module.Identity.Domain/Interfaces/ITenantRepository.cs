using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Identity.Domain.Interfaces;

public interface ITenantRepository : IRepository
{
    Task<Tenant?> GetByCodigoConviteAsync(string codigoConvite, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
}
