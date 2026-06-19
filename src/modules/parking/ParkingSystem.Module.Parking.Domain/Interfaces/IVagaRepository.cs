using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Domain.Interfaces;

public interface IVagaRepository : IRepository
{
    Task<Vaga?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Vaga?> GetByNumeroAsync(string numero, CancellationToken ct = default);
    Task<IEnumerable<Vaga>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Vaga vaga, CancellationToken ct = default);
    void Update(Vaga vaga);
}
