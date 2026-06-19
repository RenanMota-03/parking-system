using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Module.Parking.Application.Reserva.Commands;
using ParkingSystem.Module.Parking.Application.Reserva.Queries;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class ReservaEndpoints
{
    public static void MapReservaEndpoints(this IEndpointRouteBuilder app)
    {
        var reservas = app.MapGroup("/api/reservas").WithTags("Reservas");

        reservas.MapGet("/", GetAllAsync).WithSummary("Lista todas as reservas")
                .RequireAuthorization(p => p.RequireRole("Admin"));

        reservas.MapGet("/{id:long}", GetByIdAsync).WithSummary("Busca reserva por id")
                .RequireAuthorization(p => p.RequireRole("Admin", "Cliente"));

        reservas.MapPost("/", CriarAsync).WithSummary("Cria uma nova reserva")
                .RequireAuthorization(p => p.RequireRole("Admin", "Cliente"));

        reservas.MapDelete("/{id:long}", CancelarAsync).WithSummary("Cancela uma reserva")
                .RequireAuthorization(p => p.RequireRole("Admin", "Cliente"));
    }

    private static async Task<IResult> GetAllAsync(
        [FromServices] IReservaQueries queries,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var reservas = await queries.GetAllAsync(page, pageSize);
        return Results.Ok(reservas);
    }

    private static async Task<IResult> GetByIdAsync(
        long id,
        [FromServices] IReservaQueries queries)
    {
        var reserva = await queries.GetByIdAsync(id);
        return reserva is null ? Results.NotFound() : Results.Ok(reserva);
    }

    private static async Task<IResult> CriarAsync(
        [FromBody] CriarReservaRequest request,
        [FromServices] ICommandHandler<CriarReservaCommand, ValidationResult> handler,
        [FromServices] IReservaQueries queries)
    {
        var command = new CriarReservaCommand(request.VagaId, request.UsuarioId, request.DataAgendada, request.DataLimite);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        var reserva = await queries.GetByIdAsync(result.Data?.id);
        return Results.Created($"/api/reservas/{result.Data?.id}", reserva);
    }

    private static async Task<IResult> CancelarAsync(
        long id,
        [FromServices] ICommandHandler<CancelarReservaCommand, ValidationResult> handler)
    {
        var command = new CancelarReservaCommand(id);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.NoContent();
    }

    public record CriarReservaRequest(long VagaId, string UsuarioId, DateTime DataAgendada, DateTime DataLimite);
}
