using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Repository;

public class VagaRepository(ParkingDbContext context) : IVagaRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Vaga?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.Vagas.FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<Vaga?> GetByNumeroAsync(string numero, CancellationToken ct = default)
        => await context.Vagas.FirstOrDefaultAsync(v => v.Numero == numero, ct);

    public async Task<IEnumerable<Vaga>> GetAllAsync(CancellationToken ct = default)
        => await context.Vagas.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(Vaga vaga, CancellationToken ct = default)
        => await context.Vagas.AddAsync(vaga, ct);

    public void Update(Vaga vaga)
        => context.Vagas.Update(vaga);
}
