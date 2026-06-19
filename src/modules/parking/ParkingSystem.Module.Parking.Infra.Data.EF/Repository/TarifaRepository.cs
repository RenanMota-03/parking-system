using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Repository;

public class TarifaRepository(ParkingDbContext context) : ITarifaRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Tarifa?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.Tarifas.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Tarifa?> GetVigenteByTipoVagaAsync(TipoVaga tipoVaga, CancellationToken ct = default)
        => await context.Tarifas
            .Where(t => t.TipoVaga == tipoVaga && (t.VigenteAte == null || t.VigenteAte >= DateTime.UtcNow))
            .OrderBy(t => t.VigenteAte == null ? 0 : 1)
            .FirstOrDefaultAsync(ct);

    public async Task<IEnumerable<Tarifa>> GetAllAsync(CancellationToken ct = default)
        => await context.Tarifas.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Tarifa tarifa, CancellationToken ct = default)
        => await context.Tarifas.AddAsync(tarifa, ct);

    public void Update(Tarifa tarifa)
        => context.Tarifas.Update(tarifa);
}
