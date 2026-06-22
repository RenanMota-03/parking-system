using Microsoft.AspNetCore.Identity;
using NSubstitute;
using ParkingSystem.Module.Identity.Application.Usuario.Commands;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Interfaces;
using ParkingSystem.Shared.Core.Data;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Identity;

public class RegistrarUsuarioCommandHandlerTests
{
    private readonly IUsuarioRepository _repo = Substitute.For<IUsuarioRepository>();
    private readonly ITenantRepository _tenantRepo = Substitute.For<ITenantRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly RegistrarUsuarioCommandHandler _sut;

    private static readonly Tenant DemoTenant = new("Demo", "CONVITE1");

    public RegistrarUsuarioCommandHandlerTests()
    {
        _repo.UnitOfWork.Returns(_uow);
        _uow.Commit().Returns(true);
        _sut = new RegistrarUsuarioCommandHandler(_repo, _tenantRepo);
    }

    // ── Validação de comando ──────────────────────────────────────────────────

    [Fact]
    public async Task Handle_NomeVazio_RetornaErroDeValidacao()
    {
        var command = new RegistrarUsuarioCommand("", "joao@parking.com", "Senha@123", "CONVITE1");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        await _repo.DidNotReceive().AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SenhaCurta_RetornaErroDeValidacao()
    {
        var command = new RegistrarUsuarioCommand("Joao", "joao@parking.com", "abc", "CONVITE1");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("6 caracteres"));
    }

    // ── Código de convite inválido ────────────────────────────────────────────

    [Fact]
    public async Task Handle_CodigoConviteInvalido_RetornaErro()
    {
        _tenantRepo.GetByCodigoConviteAsync("INVALIDO", Arg.Any<CancellationToken>())
                   .Returns((Tenant?)null);
        var command = new RegistrarUsuarioCommand("Joao", "joao@parking.com", "Senha@123", "INVALIDO");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("convite"));
        await _repo.DidNotReceive().AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    // ── Email duplicado ───────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_EmailJaCadastrado_RetornaErro()
    {
        _tenantRepo.GetByCodigoConviteAsync("CONVITE1", Arg.Any<CancellationToken>())
                   .Returns(DemoTenant);
        _repo.ExisteEmailAsync("joao@parking.com", Arg.Any<CancellationToken>()).Returns(true);
        var command = new RegistrarUsuarioCommand("Joao", "joao@parking.com", "Senha@123", "CONVITE1");

        var result = await _sut.Handle(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("e-mail"));
        await _repo.DidNotReceive().AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
    }

    // ── Caminho feliz ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Handle_DadosValidos_UsuarioCriado()
    {
        _tenantRepo.GetByCodigoConviteAsync("CONVITE1", Arg.Any<CancellationToken>())
                   .Returns(DemoTenant);
        _repo.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var command = new RegistrarUsuarioCommand("Joao Silva", "joao@parking.com", "Senha@123", "CONVITE1");

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        await _repo.Received(1).AddAsync(Arg.Any<Usuario>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_DadosValidos_SenhaArmazenadaComoHash()
    {
        _tenantRepo.GetByCodigoConviteAsync("CONVITE1", Arg.Any<CancellationToken>())
                   .Returns(DemoTenant);
        _repo.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        Usuario? usuarioCriado = null;
        await _repo.AddAsync(Arg.Do<Usuario>(u => usuarioCriado = u), Arg.Any<CancellationToken>());

        var command = new RegistrarUsuarioCommand("Joao Silva", "joao@parking.com", "Senha@123", "CONVITE1");
        await _sut.Handle(command);

        Assert.NotNull(usuarioCriado);
        Assert.NotEqual("Senha@123", usuarioCriado!.SenhaHash);

        var hasher = new PasswordHasher<Usuario>();
        var verificacao = hasher.VerifyHashedPassword(null!, usuarioCriado.SenhaHash, "Senha@123");
        Assert.Equal(PasswordVerificationResult.Success, verificacao);
    }

    [Fact]
    public async Task Handle_DadosValidos_RetornaIdEmailRoleCliente()
    {
        _tenantRepo.GetByCodigoConviteAsync("CONVITE1", Arg.Any<CancellationToken>())
                   .Returns(DemoTenant);
        _repo.ExisteEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var command = new RegistrarUsuarioCommand("Joao Silva", "joao@parking.com", "Senha@123", "CONVITE1");

        var result = await _sut.Handle(command);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Data);
        Assert.Equal("joao@parking.com", (string)result.Data.email);
        Assert.Equal("Cliente", (string)result.Data.role);
    }
}
