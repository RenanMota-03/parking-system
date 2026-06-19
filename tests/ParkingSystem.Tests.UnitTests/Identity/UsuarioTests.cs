using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Shared.Core.Exceptions;
using Xunit;

namespace ParkingSystem.Tests.UnitTests.Identity;

public class UsuarioTests
{
    private static Usuario CriarUsuario(
        string nome     = "Joao Silva",
        string email    = "joao@parking.com",
        string hash     = "hash_qualquer",
        Role   role     = Role.Cliente)
        => new(nome, email, hash, role);

    // ── Criação ───────────────────────────────────────────────────────────────

    [Fact]
    public void Criar_ComDadosValidos_PropriedadesPreenchidas()
    {
        var usuario = CriarUsuario();

        Assert.Equal("Joao Silva", usuario.Nome);
        Assert.Equal("joao@parking.com", usuario.Email);
        Assert.Equal(Role.Cliente, usuario.Role);
        Assert.False(usuario.IsDeleted);
    }

    [Fact]
    public void Criar_EmailComMaiusculas_NormalizaParaMinusculas()
    {
        var usuario = CriarUsuario(email: "JOAO@PARKING.COM");

        Assert.Equal("joao@parking.com", usuario.Email);
    }

    [Fact]
    public void Criar_NomeVazio_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() => CriarUsuario(nome: ""));
    }

    [Fact]
    public void Criar_NomeMaiorQue100Chars_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() => CriarUsuario(nome: new string('A', 101)));
    }

    [Fact]
    public void Criar_EmailVazio_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() => CriarUsuario(email: ""));
    }

    [Fact]
    public void Criar_SenhaHashVazia_LancaExcecao()
    {
        Assert.Throws<EntityValidationException>(() => CriarUsuario(hash: ""));
    }

    // ── AtualizarSenha ────────────────────────────────────────────────────────

    [Fact]
    public void AtualizarSenha_HashValido_SenhaHashAtualizada()
    {
        var usuario = CriarUsuario();
        var novoHash = "novo_hash_bcrypt";

        usuario.AtualizarSenha(novoHash);

        Assert.Equal(novoHash, usuario.SenhaHash);
    }

    [Fact]
    public void AtualizarSenha_HashVazio_LancaExcecao()
    {
        var usuario = CriarUsuario();

        Assert.Throws<EntityValidationException>(() => usuario.AtualizarSenha(""));
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(Role.Admin)]
    [InlineData(Role.Operador)]
    [InlineData(Role.Cliente)]
    public void Criar_ComQualquerRole_RoleCorreta(Role role)
    {
        var usuario = CriarUsuario(role: role);

        Assert.Equal(role, usuario.Role);
    }
}
