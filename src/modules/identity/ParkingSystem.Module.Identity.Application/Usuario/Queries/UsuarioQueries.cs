using Dapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Identity.Application.Usuario.Models;
using ParkingSystem.Module.Identity.Infra.Data.EF;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Identity.Application.Usuario.Queries;

public interface IUsuarioQueries
{
    Task<UsuarioLoginDto?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<PagedResult<UsuarioListDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
}

public class UsuarioQueries(IdentityDbContext context) : IUsuarioQueries
{
    public async Task<UsuarioLoginDto?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string sql = """
            SELECT id, nome, email, senha_hash, role, tenant_id
            FROM usuarios
            WHERE email = @email AND is_deleted = false
            """;

        return await conn.QueryFirstOrDefaultAsync<UsuarioLoginDto>(sql, new { email = email.ToLowerInvariant() });
    }

    public async Task<PagedResult<UsuarioListDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string countSql = "SELECT COUNT(*) FROM usuarios WHERE is_deleted = false";
        var total = await conn.ExecuteScalarAsync<int>(countSql);

        const string sql = """
            SELECT id, nome, email, role, created_at
            FROM usuarios
            WHERE is_deleted = false
            ORDER BY created_at DESC
            LIMIT @pageSize OFFSET @offset
            """;

        var items = await conn.QueryAsync<UsuarioListDto>(sql, new { pageSize, offset = (page - 1) * pageSize });

        return new PagedResult<UsuarioListDto>(items, total, page, pageSize);
    }
}
