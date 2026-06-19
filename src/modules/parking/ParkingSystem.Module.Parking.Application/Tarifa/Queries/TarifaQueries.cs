using Dapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Application.Tarifa.Models;
using ParkingSystem.Module.Parking.Infra.Data.EF;

namespace ParkingSystem.Module.Parking.Application.Tarifa.Queries;

public interface ITarifaQueries
{
    Task<IEnumerable<TarifaDto>> GetAllAsync(CancellationToken ct = default);
    Task<TarifaDto?> GetByIdAsync(long id, CancellationToken ct = default);
}

public class TarifaQueries(ParkingDbContext context) : ITarifaQueries
{
    public async Task<IEnumerable<TarifaDto>> GetAllAsync(CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string sql = """
            SELECT id, tipo_vaga, valor_hora, valor_diaria, valor_mensal, vigente_ate, created_at
            FROM tarifas
            WHERE is_deleted = false
            ORDER BY tipo_vaga, created_at DESC
            """;

        return await conn.QueryAsync<TarifaDto>(sql);
    }

    public async Task<TarifaDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string sql = """
            SELECT id, tipo_vaga, valor_hora, valor_diaria, valor_mensal, vigente_ate, created_at
            FROM tarifas
            WHERE id = @id AND is_deleted = false
            """;

        return await conn.QueryFirstOrDefaultAsync<TarifaDto>(sql, new { id });
    }
}
