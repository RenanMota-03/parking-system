using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Identity.Infra.Data.EF.Repository;

public class UsuarioRepository(IdentityDbContext context) : IUsuarioRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default)
        => await context.Usuarios
            .AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(Usuario usuario, CancellationToken ct = default)
        => await context.Usuarios.AddAsync(usuario, ct);
}
