using Dapper;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Application.Movimentacao.Models;
using ParkingSystem.Module.Parking.Infra.Data.EF;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Application.Movimentacao.Queries;

public interface IMovimentacaoQueries
{
    Task<MovimentacaoDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<PagedResult<MovimentacaoDto>> GetAbertasAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<PagedResult<MovimentacaoDto>> GetTodasAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
}

public class MovimentacaoQueries(ParkingDbContext context) : IMovimentacaoQueries
{
    private const string SelectColumns = """
        m.id, m.vaga_id, v.numero as numero_vaga, m.placa_veiculo,
        m.data_entrada, m.data_saida, m.valor_total, m.pago, m.forma_pagamento
        """;

    private const string FromAbertasWhere = """
        FROM movimentacoes m
        INNER JOIN vagas v ON v.id = m.vaga_id
        WHERE m.data_saida IS NULL AND m.is_deleted = false
        """;

    private const string FromTodasWhere = """
        FROM movimentacoes m
        INNER JOIN vagas v ON v.id = m.vaga_id
        WHERE m.is_deleted = false
        """;

    public async Task<MovimentacaoDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        var sql = $"""
            SELECT {SelectColumns}
            FROM movimentacoes m
            INNER JOIN vagas v ON v.id = m.vaga_id
            WHERE m.id = @id AND m.is_deleted = false
            """;

        return await conn.QueryFirstOrDefaultAsync<MovimentacaoDto>(sql, new { id });
    }

    public async Task<PagedResult<MovimentacaoDto>> GetAbertasAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) {FromAbertasWhere}");

        var sql = $"""
            SELECT {SelectColumns}
            {FromAbertasWhere}
            ORDER BY m.data_entrada DESC
            LIMIT @pageSize OFFSET @offset
            """;

        var items = await conn.QueryAsync<MovimentacaoDto>(sql, new { pageSize, offset = (page - 1) * pageSize });

        return new PagedResult<MovimentacaoDto>(items, total, page, pageSize);
    }

    public async Task<PagedResult<MovimentacaoDto>> GetTodasAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var conn = context.Database.GetDbConnection();

        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) {FromTodasWhere}");

        var sql = $"""
            SELECT {SelectColumns}
            {FromTodasWhere}
            ORDER BY m.data_entrada DESC
            LIMIT @pageSize OFFSET @offset
            """;

        var items = await conn.QueryAsync<MovimentacaoDto>(sql, new { pageSize, offset = (page - 1) * pageSize });

        return new PagedResult<MovimentacaoDto>(items, total, page, pageSize);
    }
}
