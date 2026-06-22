using NSubstitute;
using ParkingSystem.Module.Parking.Application.Vaga.Commands;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;
using ParkingSystem.Shared.Core.Services;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Parking;

public class CadastrarVagaCommandHandlerTests
{
    private readonly IVagaRepository _repo = Substitute.For<IVagaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly CadastrarVagaCommandHandler _sut;

    public CadastrarVagaCommandHandlerTests()
    {
        _repo.UnitOfWork.Returns(_uow);
        _uow.Commit().Returns(true);
        _sut = new CadastrarVagaCommandHandler(_repo, new SystemTenantProvider());
    }

    // ── Validação de comando ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NumeroVazio_RetornaErroDeValidacao()
    {
        var command = new CadastrarVagaCommand("", TipoVaga.Carro);

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Vaga>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NumeroMuitoLongo_RetornaErroDeValidacao()
    {
        var command = new CadastrarVagaCommand("A12345678901", TipoVaga.Carro);

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("10 caracteres"));
    }

    // ── Número duplicado ──────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NumeroJaExiste_RetornaErro()
    {
        _repo.GetByNumeroAsync("A1", Arg.Any<CancellationToken>())
             .Returns(new Vaga(1L, "A1", TipoVaga.Carro));
        var command = new CadastrarVagaCommand("A1", TipoVaga.Carro);

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("A1"));
        await _repo.DidNotReceive().AddAsync(Arg.Any<Vaga>(), Arg.Any<CancellationToken>());
    }

    // ── Caminho feliz ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DadosValidos_VagaCriada()
    {
        _repo.GetByNumeroAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Vaga?)null);
        var command = new CadastrarVagaCommand("B2", TipoVaga.Moto);

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        await _repo.Received(1).AddAsync(Arg.Any<Vaga>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_DadosValidos_RetornaIdENumero()
    {
        _repo.GetByNumeroAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Vaga?)null);
        var command = new CadastrarVagaCommand("C1", TipoVaga.Caminhonete);

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal("C1", (string)result.Data.numero);
    }

    [Theory]
    [InlineData(TipoVaga.Carro)]
    [InlineData(TipoVaga.Moto)]
    [InlineData(TipoVaga.Caminhonete)]
    [InlineData(TipoVaga.DeficienteOuIdoso)]
    public async Task Handle_TodosTiposDeVaga_Aceitos(TipoVaga tipo)
    {
        _repo.GetByNumeroAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Vaga?)null);
        var command = new CadastrarVagaCommand("X1", tipo);

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
    }
}
