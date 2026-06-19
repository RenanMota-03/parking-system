using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Shared.Core.Data;

namespace ParkingSystem.Module.Parking.Domain.Interfaces;

public interface IMovimentacaoRepository : IRepository
{
    Task<Movimentacao?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<Movimentacao?> GetAbertaByPlacaAsync(string placa, CancellationToken ct = default);
    Task<Movimentacao?> GetAbertaByVagaIdAsync(long vagaId, CancellationToken ct = default);
    Task AddAsync(Movimentacao movimentacao, CancellationToken ct = default);
    void Update(Movimentacao movimentacao);
}
