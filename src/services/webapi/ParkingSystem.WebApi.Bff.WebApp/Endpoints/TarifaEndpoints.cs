using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Module.Parking.Application.Tarifa.Commands;
using ParkingSystem.Module.Parking.Application.Tarifa.Queries;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class TarifaEndpoints
{
    public static void MapTarifaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tarifas")
            .WithTags("Tarifas");

        group.MapGet("/", ListTarifasAsync).WithSummary("Lista todas as tarifas")
             .RequireAuthorization(p => p.RequireRole("Admin", "Operador"));

        group.MapGet("/{id:long}", GetTarifaAsync).WithSummary("Busca tarifa por id")
             .RequireAuthorization(p => p.RequireRole("Admin", "Operador"));

        group.MapPost("/", CadastrarTarifaAsync).WithSummary("Cadastra uma nova tarifa")
             .RequireAuthorization(p => p.RequireRole("Admin"));

        group.MapPut("/{id:long}", AtualizarTarifaAsync).WithSummary("Atualiza os valores de uma tarifa")
             .RequireAuthorization(p => p.RequireRole("Admin"));
    }

    private static async Task<IResult> ListTarifasAsync(
        [FromServices] ITarifaQueries queries)
    {
        var tarifas = await queries.GetAllAsync();
        return Results.Ok(tarifas);
    }

    private static async Task<IResult> GetTarifaAsync(
        long id,
        [FromServices] ITarifaQueries queries)
    {
        var tarifa = await queries.GetByIdAsync(id);
        return tarifa is null ? Results.NotFound() : Results.Ok(tarifa);
    }

    private static async Task<IResult> CadastrarTarifaAsync(
        [FromBody] CadastrarTarifaRequest request,
        [FromServices] ICommandHandler<CadastrarTarifaCommand, ValidationResult> handler)
    {
        var command = new CadastrarTarifaCommand(
            request.TipoVaga,
            request.ValorHora,
            request.ValorDiaria,
            request.ValorMensal,
            request.VigenteAte);

        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Created($"/api/tarifas/{result.Data?.id}", result.Data);
    }

    private static async Task<IResult> AtualizarTarifaAsync(
        long id,
        [FromBody] AtualizarTarifaRequest request,
        [FromServices] ICommandHandler<AtualizarTarifaCommand, ValidationResult> handler)
    {
        var command = new AtualizarTarifaCommand(
            id,
            request.ValorHora,
            request.ValorDiaria,
            request.ValorMensal,
            request.VigenteAte);

        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Ok(new { id });
    }

    public record CadastrarTarifaRequest(
        TipoVaga TipoVaga,
        decimal ValorHora,
        decimal ValorDiaria,
        decimal ValorMensal,
        DateTime? VigenteAte = null);

    public record AtualizarTarifaRequest(
        decimal ValorHora,
        decimal ValorDiaria,
        decimal ValorMensal,
        DateTime? VigenteAte = null);
}
