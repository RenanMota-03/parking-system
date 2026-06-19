using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Domain.Interfaces;

public interface IReservaRepository : IRepository
{
    Task<Reserva?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IEnumerable<Reserva>> GetAtivasByVagaIdAsync(long vagaId, CancellationToken ct = default);
    Task<IEnumerable<Reserva>> GetAtivasByUsuarioIdAsync(string usuarioId, CancellationToken ct = default);
    Task AddAsync(Reserva reserva, CancellationToken ct = default);
    void Update(Reserva reserva);
}
