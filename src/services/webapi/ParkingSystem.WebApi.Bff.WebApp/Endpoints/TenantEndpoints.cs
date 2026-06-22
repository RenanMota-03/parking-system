using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Module.Identity.Application.Tenant.Commands;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class TenantEndpoints
{
    public static void MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var tenants = app.MapGroup("/api/tenants").WithTags("Tenants");

        tenants.MapPost("/", CriarTenantAsync)
            .WithSummary("Cria um novo tenant (estacionamento)")
            .RequireAuthorization(p => p.RequireRole("SuperAdmin"));
    }

    private static async Task<IResult> CriarTenantAsync(
        [FromBody] CriarTenantRequest request,
        [FromServices] ICommandHandler<CriarTenantCommand, ValidationResult> handler)
    {
        var command = new CriarTenantCommand(request.Nome);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Created($"/api/tenants/{result.Data?.id}", new
        {
            id            = result.Data?.id,
            nome          = result.Data?.nome,
            codigoConvite = result.Data?.codigoConvite,
        });
    }

    public record CriarTenantRequest(string Nome);
}
