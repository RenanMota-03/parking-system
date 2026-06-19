using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Domain.Interfaces;

public interface ITarifaRepository : IRepository
{
    Task<Tarifa?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Tarifa?> GetVigenteByTipoVagaAsync(TipoVaga tipoVaga, CancellationToken ct = default);
    Task<IEnumerable<Tarifa>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Tarifa tarifa, CancellationToken ct = default);
    void Update(Tarifa tarifa);
}
