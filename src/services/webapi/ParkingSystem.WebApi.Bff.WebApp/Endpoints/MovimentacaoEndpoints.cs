using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Module.Parking.Application.Movimentacao.Commands;
using ParkingSystem.Module.Parking.Application.Movimentacao.Queries;
using ParkingSystem.Module.Parking.Domain.Enums;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class MovimentacaoEndpoints
{
    public static void MapMovimentacaoEndpoints(this IEndpointRouteBuilder app)
    {
        var fluxo = app.MapGroup("/api/fluxo").WithTags("Fluxo");
        var movimentacoes = app.MapGroup("/api/movimentacoes").WithTags("Movimentacoes");

        fluxo.MapPost("/entrada", RegistrarEntradaAsync).WithSummary("Registra a entrada de um veículo")
             .RequireAuthorization(p => p.RequireRole("Admin", "Operador"));

        fluxo.MapPost("/saida", RegistrarSaidaAsync).WithSummary("Registra a saída e calcula o valor")
             .RequireAuthorization(p => p.RequireRole("Admin", "Operador"));

        fluxo.MapPost("/pagamento", PagarAsync).WithSummary("Processa o pagamento")
             .RequireAuthorization(p => p.RequireRole("Admin", "Operador"));

        movimentacoes.MapGet("/", ListAsync).WithSummary("Lista movimentações (aberta=true filtra apenas abertas)")
                     .RequireAuthorization(p => p.RequireRole("Admin", "Operador"));

        movimentacoes.MapGet("/{id:long}", GetByIdAsync).WithSummary("Busca movimentação por id")
                     .RequireAuthorization(p => p.RequireRole("Admin", "Operador"));
    }

    private static async Task<IResult> RegistrarEntradaAsync(
        [FromBody] RegistrarEntradaRequest request,
        [FromServices] ICommandHandler<RegistrarEntradaCommand, ValidationResult> handler)
    {
        var command = new RegistrarEntradaCommand(request.VagaId, request.PlacaVeiculo);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Created($"/api/movimentacoes/{result.Data?.id}", result.Data);
    }

    private static async Task<IResult> RegistrarSaidaAsync(
        [FromBody] RegistrarSaidaRequest request,
        [FromServices] ICommandHandler<RegistrarSaidaCommand, ValidationResult> handler,
        [FromServices] IMovimentacaoQueries queries)
    {
        var command = new RegistrarSaidaCommand(request.PlacaVeiculo);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        var movimentacao = await queries.GetByIdAsync(result.Data?.id);
        return Results.Ok(movimentacao);
    }

    private static async Task<IResult> PagarAsync(
        [FromBody] PagarRequest request,
        [FromServices] ICommandHandler<PagarCommand, ValidationResult> handler)
    {
        var command = new PagarCommand(request.MovimentacaoId, request.FormaPagamento);
        var result = await handler.Handle(command);

        if (!result.IsValid) return Results.UnprocessableEntity(result.Errors);

        return Results.Ok(new { id = request.MovimentacaoId });
    }

    private static async Task<IResult> ListAsync(
        [FromServices] IMovimentacaoQueries queries,
        [FromQuery] bool? aberta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = aberta == true
            ? await queries.GetAbertasAsync(page, pageSize)
            : await queries.GetTodasAsync(page, pageSize);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetByIdAsync(
        long id,
        [FromServices] IMovimentacaoQueries queries)
    {
        var movimentacao = await queries.GetByIdAsync(id);
        return movimentacao is null ? Results.NotFound() : Results.Ok(movimentacao);
    }

    public record RegistrarEntradaRequest(long VagaId, string PlacaVeiculo);
    public record RegistrarSaidaRequest(string PlacaVeiculo);
    public record PagarRequest(long MovimentacaoId, FormaPagamento FormaPagamento);
}
