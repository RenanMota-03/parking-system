using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Module.Identity.Application.Usuario.Commands;
using ParkingSystem.Module.Identity.Application.Usuario.Queries;
using ParkingSystem.Module.Identity.Domain.Enums;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class UsuariosEndpoints
{
    public static void MapUsuariosEndpoints(this IEndpointRouteBuilder app)
    {
        var usuarios = app.MapGroup("/api/usuarios").WithTags("Usuarios");

        usuarios.MapGet("/", ListAsync)
                .WithSummary("Lista todos os usuários")
                .RequireAuthorization(p => p.RequireRole("Admin"));

        usuarios.MapPost("/", CriarAsync)
                .WithSummary("Cria um novo usuário (Admin)")
                .RequireAuthorization(p => p.RequireRole("Admin"));
    }

    private static async Task<IResult> ListAsync(
        [FromServices] IUsuarioQueries queries,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await queries.GetAllAsync(page, pageSize, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> CriarAsync(
        [FromBody] CriarUsuarioRequest request,
        [FromServices] ICommandHandler<CriarUsuarioPorAdminCommand, ValidationResult> handler,
        CancellationToken ct = default)
    {
        var command = new CriarUsuarioPorAdminCommand(request.Nome, request.Email, request.Senha, request.Role);
        var result  = await handler.Handle(command, ct);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Created($"/api/usuarios/{result.Data?.id}", new
        {
            id    = result.Data?.id,
            email = result.Data?.email,
            role  = result.Data?.role,
        });
    }

    public record CriarUsuarioRequest(string Nome, string Email, string Senha, Role Role);
}
