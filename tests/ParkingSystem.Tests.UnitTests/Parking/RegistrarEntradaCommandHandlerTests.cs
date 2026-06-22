using NSubstitute;
using ParkingSystem.Module.Parking.Application.Movimentacao.Commands;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Parking;

public class RegistrarEntradaCommandHandlerTests
{
    private readonly IVagaRepository _vagaRepo = Substitute.For<IVagaRepository>();
    private readonly IMovimentacaoRepository _movRepo = Substitute.For<IMovimentacaoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly RegistrarEntradaCommandHandler _sut;

    public RegistrarEntradaCommandHandlerTests()
    {
        _movRepo.UnitOfWork.Returns(_uow);
        _uow.Commit().Returns(true);
        _sut = new RegistrarEntradaCommandHandler(_vagaRepo, _movRepo);
    }

    // ── Validação de comando ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_VagaIdZero_RetornaErroDeValidacao()
    {
        var command = new RegistrarEntradaCommand(0, "ABC1234");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        await _vagaRepo.DidNotReceive().GetByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PlacaVazia_RetornaErroDeValidacao()
    {
        var command = new RegistrarEntradaCommand(1, "");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        await _movRepo.DidNotReceive().AddAsync(Arg.Any<Movimentacao>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PlacaMuitoLonga_RetornaErroDeValidacao()
    {
        var command = new RegistrarEntradaCommand(1, "ABC123456");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("8 caracteres"));
    }

    // ── Regras de negócio ─────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_VagaNaoEncontrada_RetornaErro()
    {
        _vagaRepo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Vaga?)null);
        var command = new RegistrarEntradaCommand(99, "ABC1234");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("99"));
        await _movRepo.DidNotReceive().AddAsync(Arg.Any<Movimentacao>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_PlacaComEntradaAberta_RetornaErro()
    {
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Vaga(1L, "A1", TipoVaga.Carro));
        _movRepo.GetAbertaByPlacaAsync("ABC1234", Arg.Any<CancellationToken>())
                .Returns(new Movimentacao(1L, 1L, "ABC1234"));
        var command = new RegistrarEntradaCommand(1, "ABC1234");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("ABC1234"));
        await _movRepo.DidNotReceive().AddAsync(Arg.Any<Movimentacao>(), Arg.Any<CancellationToken>());
    }

    // ── Caminho feliz ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DadosValidos_MovimentacaoCriada()
    {
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Vaga(1L, "A1", TipoVaga.Carro));
        _movRepo.GetAbertaByPlacaAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((Movimentacao?)null);
        var command = new RegistrarEntradaCommand(1, "ABC1234");

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        await _movRepo.Received(1).AddAsync(Arg.Any<Movimentacao>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_DadosValidos_VagaMarcadaComoOcupada()
    {
        var vaga = new Vaga(1L, "A1", TipoVaga.Carro);
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(vaga);
        _movRepo.GetAbertaByPlacaAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((Movimentacao?)null);
        var command = new RegistrarEntradaCommand(1, "ABC1234");

        await _sut.Handle(command);

        Assert.Equal(StatusVaga.Ocupada, vaga.Status);
        _vagaRepo.Received(1).Update(vaga);
    }

    [Fact]
    public async Task Handle_DadosValidos_RetornaIdPlacaDataEntrada()
    {
        _vagaRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(new Vaga(1L, "A1", TipoVaga.Carro));
        _movRepo.GetAbertaByPlacaAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((Movimentacao?)null);
        var command = new RegistrarEntradaCommand(1, "ABC1234");

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal("ABC1234", (string)result.Data.placa);
    }
}
