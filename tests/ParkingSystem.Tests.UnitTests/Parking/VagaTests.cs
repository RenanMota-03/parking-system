using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.Exceptions;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Parking;

public class VagaTests
{
    private static Vaga CriarVaga() => new(1L, "A1", TipoVaga.Carro);

    // ── Criação ───────────────────────────────────────────────────────────────

    [Fact]
    public void Criar_ComDadosValidos_StatusDisponivel()
    {
        var vaga = CriarVaga();

        Assert.Equal("A1", vaga.Numero);
        Assert.Equal(TipoVaga.Carro, vaga.TipoVaga);
        Assert.Equal(StatusVaga.Disponivel, vaga.Status);
    }

    [Fact]
    public void Criar_NumeroVazio_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() => new Vaga(1L, "", TipoVaga.Carro));
    }

    [Fact]
    public void Criar_NumeroMaiorQue10Chars_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() => new Vaga(1L, "12345678901", TipoVaga.Carro));
    }

    // ── Ocupar ────────────────────────────────────────────────────────────────

    [Fact]
    public void Ocupar_VagaDisponivel_StatusOcupada()
    {
        var vaga = CriarVaga();

        vaga.Ocupar();

        Assert.Equal(StatusVaga.Ocupada, vaga.Status);
    }

    [Fact]
    public void Ocupar_VagaJaOcupada_LancaExcecao()
    {
        var vaga = CriarVaga();
        vaga.Ocupar();

        Assert.Throws<InvalidOperationException>(() => vaga.Ocupar());
    }

    // ── Liberar ───────────────────────────────────────────────────────────────

    [Fact]
    public void Liberar_VagaOcupada_StatusDisponivel()
    {
        var vaga = CriarVaga();
        vaga.Ocupar();

        vaga.Liberar();

        Assert.Equal(StatusVaga.Disponivel, vaga.Status);
    }

    [Fact]
    public void Liberar_VagaNaoOcupada_LancaExcecao()
    {
        var vaga = CriarVaga();

        Assert.Throws<InvalidOperationException>(() => vaga.Liberar());
    }

    // ── Reservar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Reservar_VagaDisponivel_StatusReservada()
    {
        var vaga = CriarVaga();

        vaga.Reservar();

        Assert.Equal(StatusVaga.Reservada, vaga.Status);
    }

    [Fact]
    public void Reservar_VagaOcupada_LancaExcecao()
    {
        var vaga = CriarVaga();
        vaga.Ocupar();

        Assert.Throws<InvalidOperationException>(() => vaga.Reservar());
    }

    [Fact]
    public void LiberarReserva_VagaReservada_StatusDisponivel()
    {
        var vaga = CriarVaga();
        vaga.Reservar();

        vaga.LiberarReserva();

        Assert.Equal(StatusVaga.Disponivel, vaga.Status);
    }

    // ── Manutenção ────────────────────────────────────────────────────────────

    [Fact]
    public void BloquearManutencao_QualquerStatus_StatusManutencao()
    {
        var vaga = CriarVaga();

        vaga.BloquearManutencao();

        Assert.Equal(StatusVaga.Manutencao, vaga.Status);
    }

    [Fact]
    public void DesbloquearManutencao_VagaEmManutencao_StatusDisponivel()
    {
        var vaga = CriarVaga();
        vaga.BloquearManutencao();

        vaga.DesbloquearManutencao();

        Assert.Equal(StatusVaga.Disponivel, vaga.Status);
    }

    [Fact]
    public void DesbloquearManutencao_VagaNaoEmManutencao_LancaExcecao()
    {
        var vaga = CriarVaga();

        Assert.Throws<InvalidOperationException>(() => vaga.DesbloquearManutencao());
    }
}
