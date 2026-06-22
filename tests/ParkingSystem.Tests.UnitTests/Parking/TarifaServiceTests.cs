using NSubstitute;
using ParkingSystem.Module.Parking.Application.Tarifa.Services;
using Xunit;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Module.Parking.Domain.Interfaces;

namespace ParkingSystem.Tests.UnitTests.Parking;

public class TarifaServiceTests
{
    private readonly ITarifaRepository _repo = Substitute.For<ITarifaRepository>();
    private readonly TarifaService _sut;

    private static readonly DateTime Base = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);

    public TarifaServiceTests()
    {
        _sut = new TarifaService(_repo);
    }

    [Fact]
    public async Task Calcular_MenosDe15Minutos_RetornaZero()
    {
        var saida = Base.AddMinutes(14);

        var valor = await _sut.CalcularAsync(TipoVaga.Carro, Base, saida);

        Assert.Equal(0m, valor);
    }

    [Fact]
    public async Task Calcular_Exatamente15Minutos_CobrarUmaHora()
    {
        var tarifa = new Tarifa(1L, TipoVaga.Carro, valorHora: 10m, valorDiaria: 60m, valorMensal: 300m);
        _repo.GetVigenteByTipoVagaAsync(TipoVaga.Carro, Arg.Any<CancellationToken>()).Returns(tarifa);

        var saida = Base.AddMinutes(15);

        var valor = await _sut.CalcularAsync(TipoVaga.Carro, Base, saida);

        Assert.Equal(10m, valor);
    }

    [Fact]
    public async Task Calcular_1hora30minutos_CobrarDuasHoras()
    {
        var tarifa = new Tarifa(1L, TipoVaga.Carro, valorHora: 10m, valorDiaria: 60m, valorMensal: 300m);
        _repo.GetVigenteByTipoVagaAsync(TipoVaga.Carro, Arg.Any<CancellationToken>()).Returns(tarifa);

        var saida = Base.AddHours(1).AddMinutes(30);

        var valor = await _sut.CalcularAsync(TipoVaga.Carro, Base, saida);

        Assert.Equal(20m, valor);
    }

    [Fact]
    public async Task Calcular_ValorCalculadoMaiorQueDiaria_RetornaValorDiaria()
    {
        var tarifa = new Tarifa(1L, TipoVaga.Carro, valorHora: 10m, valorDiaria: 60m, valorMensal: 300m);
        _repo.GetVigenteByTipoVagaAsync(TipoVaga.Carro, Arg.Any<CancellationToken>()).Returns(tarifa);

        var saida = Base.AddHours(10);

        var valor = await _sut.CalcularAsync(TipoVaga.Carro, Base, saida);

        Assert.Equal(60m, valor);
    }

    [Fact]
    public async Task Calcular_SemTarifaVigente_LancaExcecao()
    {
        _repo.GetVigenteByTipoVagaAsync(TipoVaga.Moto, Arg.Any<CancellationToken>()).Returns((Tarifa?)null);

        var saida = Base.AddHours(2);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CalcularAsync(TipoVaga.Moto, Base, saida));
    }
}
