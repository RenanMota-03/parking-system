using Dapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Application.Relatorio.Models;
using ParkingSystem.Module.Parking.Infra.Data.EF;

namespace ParkingSystem.Module.Parking.Application.Relatorio.Queries;

public interface IRelatorioQueries
{
    Task<ResumoFinanceiroDto> GetResumoAsync(CancellationToken ct = default);
}

public class RelatorioQueries(ParkingDbContext context) : IRelatorioQueries
{
    public async Task<ResumoFinanceiroDto> GetResumoAsync(CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        const string sql = """
            SELECT
                COALESCE((
                    SELECT SUM(valor_total)
                    FROM movimentacoes
                    WHERE pago = true AND is_deleted = false
                      AND data_saida::date = CURRENT_DATE
                ), 0) AS receita_hoje,

                COALESCE((
                    SELECT SUM(valor_total)
                    FROM movimentacoes
                    WHERE pago = true AND is_deleted = false
                ), 0) AS receita_total,

                COALESCE((
                    SELECT AVG(valor_total)
                    FROM movimentacoes
                    WHERE pago = true AND is_deleted = false AND valor_total > 0
                ), 0) AS ticket_medio,

                (
                    SELECT COUNT(*)
                    FROM movimentacoes
                    WHERE pago = true AND is_deleted = false
                      AND data_saida::date = CURRENT_DATE
                ) AS transacoes_hoje,

                (
                    SELECT COUNT(*)
                    FROM reservas
                    WHERE status IN (0, 1) AND is_deleted = false
                ) AS reservas_ativas
            """;

        return await conn.QueryFirstAsync<ResumoFinanceiroDto>(sql);
    }
}
