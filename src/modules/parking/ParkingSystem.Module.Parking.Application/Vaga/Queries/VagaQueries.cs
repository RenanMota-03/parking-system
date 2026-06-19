using Dapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Application.Vaga.Models;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Application.Vaga.Queries;

public interface IVagaQueries
{
    Task<PagedResult<VagaDto>> GetAllAsync(int? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<VagaDto?> GetByIdAsync(long id, CancellationToken ct = default);
}

public class VagaQueries(ParkingDbContext context) : IVagaQueries
{
    public async Task<PagedResult<VagaDto>> GetAllAsync(int? status = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        var where = "WHERE is_deleted = false";
        var param = new DynamicParameters();

        if (status.HasValue)
        {
            where += " AND status = @status";
            param.Add("status", status.Value);
        }

        var countSql = $"SELECT COUNT(*) FROM vagas {where}";
        var total = await conn.ExecuteScalarAsync<int>(countSql, param);

        param.Add("offset", (page - 1) * pageSize);
        param.Add("pageSize", pageSize);

        var sql = $"""
            SELECT id, numero, tipo_vaga, status, created_at, updated_at
            FROM vagas
            {where}
            ORDER BY numero
            LIMIT @pageSize OFFSET @offset
            """;

        var items = await conn.QueryAsync<VagaDto>(sql, param);

        return new PagedResult<VagaDto>(items, total, page, pageSize);
    }

    public async Task<VagaDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string sql = """
            SELECT id, numero, tipo_vaga, status, created_at, updated_at
            FROM vagas
            WHERE id = @id AND is_deleted = false
            """;

        return await conn.QueryFirstOrDefaultAsync<VagaDto>(sql, new { id });
    }
}
