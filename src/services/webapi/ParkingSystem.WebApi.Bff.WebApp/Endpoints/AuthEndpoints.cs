using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ParkingSystem.Module.Identity.Application.Usuario.Commands;
using ParkingSystem.Module.Identity.Application.Usuario.Queries;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth").WithTags("Auth");

        auth.MapPost("/registro", RegistrarAsync)
            .WithSummary("Registra um novo usuário")
            .AllowAnonymous();

        auth.MapPost("/login", LoginAsync)
            .WithSummary("Autentica e retorna um JWT")
            .AllowAnonymous();
    }

    private static async Task<IResult> RegistrarAsync(
        [FromBody] RegistroRequest request,
        [FromServices] ICommandHandler<RegistrarUsuarioCommand, ValidationResult> handler)
    {
        var command = new RegistrarUsuarioCommand(request.Nome, request.Email, request.Senha, request.CodigoConvite);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Created($"/api/auth/{result.Data?.id}", new
        {
            id    = result.Data?.id,
            email = result.Data?.email,
            role  = result.Data?.role,
        });
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] IUsuarioQueries queries,
        IConfiguration configuration)
    {
        var usuario = await queries.GetByEmailAsync(request.Email);
        if (usuario is null) return Results.Unauthorized();

        var hasher = new PasswordHasher<Usuario>();
        var verificacao = hasher.VerifyHashedPassword(null!, usuario.SenhaHash, request.Senha);
        if (verificacao == PasswordVerificationResult.Failed) return Results.Unauthorized();

        var token = GerarToken(usuario.Id, usuario.Email, usuario.Nome,
            ((Role)usuario.Role).ToString(), usuario.TenantId, configuration);

        return Results.Ok(new
        {
            access_token = token.Token,
            token_type   = "Bearer",
            expires_at   = token.ExpiresAt,
            role         = ((Role)usuario.Role).ToString(),
            name         = usuario.Nome,
        });
    }

    private static (string Token, DateTime ExpiresAt) GerarToken(
        long userId, string email, string nome, string role, long? tenantId, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secret     = jwtSection["Secret"]!;
        var issuer     = jwtSection["Issuer"]!;
        var audience   = jwtSection["Audience"]!;
        var expires    = DateTime.UtcNow.AddHours(int.Parse(jwtSection["ExpiresInHours"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name,  nome),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(ClaimTypes.Role,               role),
        };

        if (tenantId.HasValue)
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token       = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public record RegistroRequest(string Nome, string Email, string Senha, string CodigoConvite);
    public record LoginRequest(string Email, string Senha);
}
