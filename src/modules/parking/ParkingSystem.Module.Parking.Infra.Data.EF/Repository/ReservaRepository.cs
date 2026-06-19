using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Repository;

public class ReservaRepository(ParkingDbContext context) : IReservaRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Reserva?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.Reservas
            .Include(r => r.Vaga)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Reserva>> GetAtivasByVagaIdAsync(long vagaId, CancellationToken ct = default)
        => await context.Reservas
            .Where(r => r.VagaId == vagaId &&
                        (r.Status == StatusReserva.Pendente || r.Status == StatusReserva.Confirmada))
            .ToListAsync(ct);

    public async Task<IEnumerable<Reserva>> GetAtivasByUsuarioIdAsync(string usuarioId, CancellationToken ct = default)
        => await context.Reservas
            .Where(r => r.UsuarioId == usuarioId &&
                        (r.Status == StatusReserva.Pendente || r.Status == StatusReserva.Confirmada))
            .ToListAsync(ct);

    public async Task AddAsync(Reserva reserva, CancellationToken ct = default)
        => await context.Reservas.AddAsync(reserva, ct);

    public void Update(Reserva reserva)
        => context.Reservas.Update(reserva);
}
