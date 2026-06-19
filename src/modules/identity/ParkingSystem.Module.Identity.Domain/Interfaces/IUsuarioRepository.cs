using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Identity.Domain.Interfaces;

public interface IUsuarioRepository : IRepository
{
    Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Usuario usuario, CancellationToken ct = default);
}
