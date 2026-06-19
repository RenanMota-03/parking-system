using Microsoft.EntityFrameworkCore;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Repository;

public class MovimentacaoRepository(ParkingDbContext context) : IMovimentacaoRepository
{
    public IUnitOfWork UnitOfWork => context;

    public async Task<Movimentacao?> GetByIdAsync(long id, CancellationToken ct = default)
        => await context.Movimentacoes
            .Include(m => m.Vaga)
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Movimentacao?> GetAbertaByPlacaAsync(string placa, CancellationToken ct = default)
        => await context.Movimentacoes
            .Include(m => m.Vaga)
            .FirstOrDefaultAsync(m => m.PlacaVeiculo == placa.ToUpperInvariant() && m.DataSaida == null, ct);

    public async Task<Movimentacao?> GetAbertaByVagaIdAsync(long vagaId, CancellationToken ct = default)
        => await context.Movimentacoes
            .FirstOrDefaultAsync(m => m.VagaId == vagaId && m.DataSaida == null, ct);

    public async Task AddAsync(Movimentacao movimentacao, CancellationToken ct = default)
        => await context.Movimentacoes.AddAsync(movimentacao, ct);

    public void Update(Movimentacao movimentacao)
        => context.Movimentacoes.Update(movimentacao);
}
