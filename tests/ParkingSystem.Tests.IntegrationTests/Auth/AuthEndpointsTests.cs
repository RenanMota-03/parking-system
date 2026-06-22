using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ParkingSystem.Tests.IntegrationTests.Auth;

[Collection("IntegrationTests")]
public class AuthEndpointsTests(ParkingWebAppFactory factory) : IClassFixture<ParkingWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private static readonly string Code = ParkingWebAppFactory.TestInviteCode;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // ── Registro ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Registro_DadosValidos_Retorna201()
    {
        var request = new { nome = "Maria Teste", email = $"maria_{Guid.NewGuid():N}@test.com", senha = "Senha@123", codigoConvite = Code };

        var response = await _client.PostAsJsonAsync("/api/auth/registro", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Registro_EmailDuplicado_Retorna422()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var request = new { nome = "Dup User", email, senha = "Senha@123", codigoConvite = Code };

        await _client.PostAsJsonAsync("/api/auth/registro", request);
        var response = await _client.PostAsJsonAsync("/api/auth/registro", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Registro_SenhaCurta_Retorna422()
    {
        var request = new { nome = "Curto", email = "curto@test.com", senha = "abc", codigoConvite = Code };

        var response = await _client.PostAsJsonAsync("/api/auth/registro", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_CredenciaisValidas_RetornaTokenJwt()
    {
        var email = $"login_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/registro", new { nome = "Login User", email, senha = "Senha@123", codigoConvite = Code });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(json.TryGetProperty("access_token", out var token));
        Assert.False(string.IsNullOrWhiteSpace(token.GetString()));
    }

    [Fact]
    public async Task Login_SenhaErrada_Retorna401()
    {
        var email = $"wrong_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/registro", new { nome = "Wrong Pass", email, senha = "Senha@123", codigoConvite = Code });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "SenhaErrada!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UsuarioNaoExiste_Retorna401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "naoexiste@test.com", senha = "Senha@123" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Endpoint protegido ────────────────────────────────────────────────────

    [Fact]
    public async Task ListarVagas_SemToken_Retorna401()
    {
        var response = await _client.GetAsync("/api/vagas");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ListarVagas_ComTokenCliente_Retorna200()
    {
        var email = $"cli_{Guid.NewGuid():N}@test.com";
        await _client.PostAsJsonAsync("/api/auth/registro", new { nome = "Cliente", email, senha = "Senha@123", codigoConvite = Code });
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new { email, senha = "Senha@123" });
        var loginBody = await loginResp.Content.ReadAsStringAsync();
        var token = JsonDocument.Parse(loginBody).RootElement.GetProperty("access_token").GetString();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/vagas");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
