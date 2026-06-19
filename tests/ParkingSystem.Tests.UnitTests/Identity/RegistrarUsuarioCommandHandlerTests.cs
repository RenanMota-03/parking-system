using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ParkingSystem.Module.Identity.Application.Usuario.Commands;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Module.Identity.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Identity;

public class RegistrarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly RegistrarUsuarioCommandHandler _sut;

    public RegistrarUsuarioCommandHandlerTests()
    {
        _repo.UnitOfWork.Returns(_uow);
        _uow.Commit().Returns(true);
        _sut = new RegistrarUsuarioCommandHandler(_repo);
    }

    // ── Validação de comando ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NomeVazio_RetornaErroDeValidacao()
    {
        var command = new RegistrarUsuarioCommand("", "joao@parking.com", "Senha@123", Role.Cliente);

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SenhaCurta_RetornaErroDeValidacao()
    {
        var command = new RegistrarUsuarioCommand("Joao", "joao@parking.com", "abc", Role.Cliente);

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("6 caracteres"));
    }

    // ── Email duplicado ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmailJaCadastrado_RetornaErro()
    {
        _repo.ExisteEmailAsync("joao@parking.com", Arg.Any<CancellationToken>()).Returns(true);
        var command = new RegistrarUsuarioCommand("Joao", "joao@parking.com", "Senha@123", Role.Cliente);

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("e-mail"));
        await _repo.DidNotReceive().AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    // ── Caminho feliz ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DadosValidos_UsuarioCriado()
    {
        _repo.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var command = new RegistrarUsuarioCommand("Joao Silva", "joao@parking.com", "Senha@123", Role.Cliente);

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        await _repo.Received(1).AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_DadosValidos_SenhaArmazenadaComoHash()
    {
        _repo.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        Usuario? usuarioCriado = null;
        await _repo.AddAsync(Arg.Do<Usuario>(u => usuarioCriado = u), Arg.Any<CancellationToken>());

        var command = new RegistrarUsuarioCommand("Joao Silva", "joao@parking.com", "Senha@123", Role.Cliente);
        await _sut.Handle(command);

        Assert.NotNull(usuarioCriado);
        Assert.NotEqual("Senha@123", usuarioCriado!.SenhaHash);

        var hasher = new PasswordHasher<Usuario>();
        var verificacao = hasher.VerifyHashedPassword(null!, usuarioCriado.SenhaHash, "Senha@123");
        Assert.Equal(PasswordVerificationResult.Success, verificacao);
    }

    [Fact]
    public async Task Handle_DadosValidos_RetornaIdEmailRole()
    {
        _repo.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var command = new RegistrarUsuarioCommand("Joao Silva", "joao@parking.com", "Senha@123", Role.Operador);

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal("joao@parking.com", (string)result.Data.email);
        Assert.Equal("Operador", (string)result.Data.role);
    }
}
