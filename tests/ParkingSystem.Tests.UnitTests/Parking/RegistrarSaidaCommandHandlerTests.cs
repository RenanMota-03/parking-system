using NSubstitute;
using ParkingSystem.Module.Parking.Application.Movimentacao.Commands;
using ParkingSystem.Module.Parking.Application.Tarifa.Services;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Parking;

public class RegistrarSaidaCommandHandlerTests
{
    private readonly IMovimentacaoRepository _movRepo = Substitute.For<IMovimentacaoRepository>();
    private readonly IVagaRepository _vagaRepo = Substitute.For<IVagaRepository>();
    private readonly ITarifaService _tarifaService = Substitute.For<ITarifaService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly RegistrarSaidaCommandHandler _sut;

    public RegistrarSaidaCommandHandlerTests()
    {
        _movRepo.UnitOfWork.Returns(_uow);
        _uow.Commit().Returns(true);
        _sut = new RegistrarSaidaCommandHandler(_movRepo, _vagaRepo, _tarifaService);
    }

    // ── Validação de comando ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_PlacaVazia_RetornaErroDeValidacao()
    {
        var command = new RegistrarSaidaCommand("");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        await _movRepo.DidNotReceive().GetAbertaByPlacaAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── Regras de negócio ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_SemEntradaAberta_RetornaErro()
    {
        _movRepo.GetAbertaByPlacaAsync("ABC1234", Arg.Any<CancellationToken>()).Returns((Movimentacao?)null);
        var command = new RegistrarSaidaCommand("ABC1234");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("ABC1234"));
    }

    [Fact]
    public async Task Handle_VagaNaoEncontrada_RetornaErro()
    {
        var movimentacao = new Movimentacao(1L, 1L, "ABC1234");
        _movRepo.GetAbertaByPlacaAsync("ABC1234", Arg.Any<CancellationToken>()).Returns(movimentacao);
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Vaga?)null);
        var command = new RegistrarSaidaCommand("ABC1234");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("Vaga"));
    }

    // ── Caminho feliz ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DadosValidos_SaidaRegistrada()
    {
        var vaga = new Vaga(1L, "A1", TipoVaga.Carro);
        vaga.Ocupar();
        var movimentacao = new Movimentacao(1L, 1L, "ABC1234");
        _movRepo.GetAbertaByPlacaAsync("ABC1234", Arg.Any<CancellationToken>()).Returns(movimentacao);
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(vaga);
        _tarifaService.CalcularAsync(TipoVaga.Carro, Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
                      .Returns(25m);
        var command = new RegistrarSaidaCommand("ABC1234");

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        _movRepo.Received(1).Update(movimentacao);
        _vagaRepo.Received(1).Update(vaga);
        await _uow.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_DadosValidos_VagaLiberada()
    {
        var vaga = new Vaga(1L, "A1", TipoVaga.Carro);
        vaga.Ocupar();
        var movimentacao = new Movimentacao(1L, 1L, "ABC1234");
        _movRepo.GetAbertaByPlacaAsync("ABC1234", Arg.Any<CancellationToken>()).Returns(movimentacao);
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(vaga);
        _tarifaService.CalcularAsync(Arg.Any<TipoVaga>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
                      .Returns(0m);
        var command = new RegistrarSaidaCommand("ABC1234");

        await _sut.Handle(command);

        Assert.Equal(StatusVaga.Disponivel, vaga.Status);
    }

    [Fact]
    public async Task Handle_DadosValidos_RetornaValorTotalEDataSaida()
    {
        var vaga = new Vaga(1L, "A1", TipoVaga.Carro);
        vaga.Ocupar();
        var movimentacao = new Movimentacao(1L, 1L, "ABC1234");
        _movRepo.GetAbertaByPlacaAsync("ABC1234", Arg.Any<CancellationToken>()).Returns(movimentacao);
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(vaga);
        _tarifaService.CalcularAsync(Arg.Any<TipoVaga>(), Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
                      .Returns(32m);
        var command = new RegistrarSaidaCommand("ABC1234");

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal(32m, (decimal)result.Data.valor_total);
        Assert.NotNull((object?)result.Data.data_saida);
    }
}
