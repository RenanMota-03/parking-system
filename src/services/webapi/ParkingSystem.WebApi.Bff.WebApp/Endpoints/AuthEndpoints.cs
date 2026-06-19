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
        var command = new RegistrarUsuarioCommand(request.Nome, request.Email, request.Senha, Role.Admin);
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
            ((Role)usuario.Role).ToString(), configuration);

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
        long userId, string email, string nome, string role, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secret     = jwtSection["Secret"]!;
        var issuer     = jwtSection["Issuer"]!;
        var audience   = jwtSection["Audience"]!;
        var expires    = DateTime.UtcNow.AddHours(int.Parse(jwtSection["ExpiresInHours"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name,  nome),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role,               role),
        };

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token       = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    public record RegistroRequest(string Nome, string Email, string Senha);
    public record LoginRequest(string Email, string Senha);
}
