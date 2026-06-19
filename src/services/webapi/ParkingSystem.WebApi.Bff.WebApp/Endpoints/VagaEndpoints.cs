using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Module.Parking.Application.Vaga.Commands;
using ParkingSystem.Module.Parking.Application.Vaga.Queries;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class VagaEndpoints
{
    public static void MapVagaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vagas")
            .WithTags("Vagas");

        group.MapGet("/", ListVagasAsync).WithSummary("Lista todas as vagas")
             .RequireAuthorization();

        group.MapPost("/", CadastrarVagaAsync).WithSummary("Cadastra uma nova vaga")
             .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapPatch("/{id:long}/status", AlterarStatusAsync).WithSummary("Altera o status de uma vaga")
             .RequireAuthorization(p => p.RequireRole("Admin"));
    }

    private static async Task<IResult> ListVagasAsync(
        [FromServices] IVagaQueries queries,
        [FromQuery] int? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var vagas = await queries.GetAllAsync(status, page, pageSize);
        return Results.Ok(vagas);
    }

    private static async Task<IResult> CadastrarVagaAsync(
        [FromBody] CadastrarVagaRequest request,
        [FromServices] ICommandHandler<CadastrarVagaCommand, ValidationResult> handler)
    {
        var command = new CadastrarVagaCommand(request.Numero, request.TipoVaga);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Created($"/api/vagas/{result.Data?.id}", result.Data);
    }

    private static async Task<IResult> AlterarStatusAsync(
        long id,
        [FromBody] AlterarStatusVagaRequest request,
        [FromServices] ICommandHandler<AlterarStatusVagaCommand, ValidationResult> handler)
    {
        var command = new AlterarStatusVagaCommand(id, request.NovoStatus);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Ok(new { id });
    }

    public record CadastrarVagaRequest(string Numero, TipoVaga TipoVaga);
    public record AlterarStatusVagaRequest(StatusVaga NovoStatus);
}
