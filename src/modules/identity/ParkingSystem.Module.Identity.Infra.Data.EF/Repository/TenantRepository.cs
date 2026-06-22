using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Identity.Infra.Data.EF.Repository;

public class TenantRepository(IdentityDbContext context) : ITenantRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Tenant?> GetByCodigoConviteAsync(string codigoConvite, CancellationToken ct = default)
        => await context.Tenants
            .FirstOrDefaultAsync(t => t.CodigoConvite == codigoConvite && t.Ativo, ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default)
        => await context.Tenants.AddAsync(tenant, ct);
}
