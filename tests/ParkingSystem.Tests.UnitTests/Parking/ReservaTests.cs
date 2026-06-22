using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.Exceptions;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Parking;

public class ReservaTests
{
    private static readonly DateTime Agendada = DateTime.UtcNow.AddHours(2);
    private static readonly DateTime Limite   = DateTime.UtcNow.AddHours(4);

    private static Reserva CriarReserva() => new(tenantId: 1, vagaId: 1, usuarioId: "user-01", Agendada, Limite);

    // ── Criação ───────────────────────────────────────────────────────────────

    [Fact]
    public void Criar_ComDadosValidos_StatusPendente()
    {
        var reserva = CriarReserva();

        Assert.Equal(StatusReserva.Pendente, reserva.Status);
        Assert.True(reserva.EstaAtiva());
    }

    [Fact]
    public void Criar_UsuarioIdVazio_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() =>
            new Reserva(1, 1, "", Agendada, Limite));
    }

    [Fact]
    public void Criar_DataLimiteMenorQueAgendada_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() =>
            new Reserva(1, 1, "user-01", Agendada, Agendada.AddMinutes(-1)));
    }

    [Fact]
    public void Criar_DataAgendadaNoPassado_LancaExcecao()
    {
        var passado = DateTime.UtcNow.AddHours(-1);

        Assert.Throws<EntityValidationException>(() =>
            new Reserva(1, 1, "user-01", passado, passado.AddHours(2)));
    }

    // ── Confirmar ─────────────────────────────────────────────────────────────

    [Fact]
    public void Confirmar_ReservaPendente_StatusConfirmada()
    {
        var reserva = CriarReserva();

        reserva.Confirmar();

        Assert.Equal(StatusReserva.Confirmada, reserva.Status);
    }

    [Fact]
    public void Confirmar_ReservaCancelada_LancaExcecao()
    {
        var reserva = CriarReserva();
        reserva.Cancelar();

        Assert.Throws<InvalidOperationException>(() => reserva.Confirmar());
    }

    // ── Cancelar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Cancelar_ReservaPendente_StatusCancelada()
    {
        var reserva = CriarReserva();

        reserva.Cancelar();

        Assert.Equal(StatusReserva.Cancelada, reserva.Status);
        Assert.False(reserva.EstaAtiva());
    }

    [Fact]
    public void Cancelar_ReservaConfirmada_StatusCancelada()
    {
        var reserva = CriarReserva();
        reserva.Confirmar();

        reserva.Cancelar();

        Assert.Equal(StatusReserva.Cancelada, reserva.Status);
    }

    [Fact]
    public void Cancelar_ReservaUtilizada_LancaExcecao()
    {
        var reserva = CriarReserva();
        reserva.Confirmar();
        reserva.Utilizar();

        Assert.Throws<InvalidOperationException>(() => reserva.Cancelar());
    }

    [Fact]
    public void Cancelar_ReservaExpirada_LancaExcecao()
    {
        var reserva = CriarReserva();
        reserva.Expirar();

        Assert.Throws<InvalidOperationException>(() => reserva.Cancelar());
    }

    // ── Expirar ───────────────────────────────────────────────────────────────

    [Fact]
    public void Expirar_ReservaPendente_StatusExpirada()
    {
        var reserva = CriarReserva();

        reserva.Expirar();

        Assert.Equal(StatusReserva.Expirada, reserva.Status);
        Assert.False(reserva.EstaAtiva());
    }

    [Fact]
    public void Expirar_ReservaConfirmada_StatusExpirada()
    {
        var reserva = CriarReserva();
        reserva.Confirmar();

        reserva.Expirar();

        Assert.Equal(StatusReserva.Expirada, reserva.Status);
    }

    [Fact]
    public void Expirar_ReservaCancelada_LancaExcecao()
    {
        var reserva = CriarReserva();
        reserva.Cancelar();

        Assert.Throws<InvalidOperationException>(() => reserva.Expirar());
    }

    // ── Utilizar ──────────────────────────────────────────────────────────────

    [Fact]
    public void Utilizar_ReservaConfirmada_StatusUtilizada()
    {
        var reserva = CriarReserva();
        reserva.Confirmar();

        reserva.Utilizar();

        Assert.Equal(StatusReserva.Utilizada, reserva.Status);
        Assert.False(reserva.EstaAtiva());
    }

    [Fact]
    public void Utilizar_ReservaExpirada_LancaExcecao()
    {
        var reserva = CriarReserva();
        reserva.Expirar();

        Assert.Throws<InvalidOperationException>(() => reserva.Utilizar());
    }
}
