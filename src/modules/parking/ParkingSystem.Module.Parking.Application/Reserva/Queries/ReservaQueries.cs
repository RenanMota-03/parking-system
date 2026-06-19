using Dapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Application.Reserva.Models;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Application.Reserva.Queries;

public interface IReservaQueries
{
    Task<ReservaDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<PagedResult<ReservaDto>> GetAtivasByUsuarioIdAsync(string usuarioId, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<PagedResult<ReservaDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
}

public class ReservaQueries(ParkingDbContext context) : IReservaQueries
{
    private const string SelectColumns = """
        r.id, r.vaga_id, v.numero as numero_vaga, r.usuario_id,
        r.data_agendada, r.data_limite, r.status, r.created_at
        """;

    private const string FromJoin = """
        FROM reservas r
        INNER JOIN vagas v ON v.id = r.vaga_id
        """;

    public async Task<ReservaDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        var sql = $"""
            SELECT {SelectColumns}
            {FromJoin}
            WHERE r.id = @id AND r.is_deleted = false
            """;

        return await conn.QueryFirstOrDefaultAsync<ReservaDto>(sql, new { id });
    }

    public async Task<PagedResult<ReservaDto>> GetAtivasByUsuarioIdAsync(string usuarioId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string where = """
            WHERE r.usuario_id = @usuarioId
              AND r.status IN (0, 1)
              AND r.is_deleted = false
            """;

        var total = await conn.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) {FromJoin} {where}", new { usuarioId });

        var sql = $"""
            SELECT {SelectColumns}
            {FromJoin}
            {where}
            ORDER BY r.data_agendada ASC
            LIMIT @pageSize OFFSET @offset
            """;

        var items = await conn.QueryAsync<ReservaDto>(sql,
            new { usuarioId, pageSize, offset = (page - 1) * pageSize });

        return new PagedResult<ReservaDto>(items, total, page, pageSize);
    }

    public async Task<PagedResult<ReservaDto>> GetAllAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string where = "WHERE r.is_deleted = false";

        var total = await conn.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) {FromJoin} {where}");

        var sql = $"""
            SELECT {SelectColumns}
            {FromJoin}
            {where}
            ORDER BY r.data_agendada ASC
            LIMIT @pageSize OFFSET @offset
            """;

        var items = await conn.QueryAsync<ReservaDto>(sql,
            new { pageSize, offset = (page - 1) * pageSize });

        return new PagedResult<ReservaDto>(items, total, page, pageSize);
    }
}
