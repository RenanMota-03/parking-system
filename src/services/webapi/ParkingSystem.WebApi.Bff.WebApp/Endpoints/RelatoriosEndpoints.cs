using ParkingSystem.Module.Parking.Application.Relatorio.Queries;

namespace ParkingSystem.WebApi.Bff.WebApp.Endpoints;

public static class RelatoriosEndpoints
{
    public static void MapRelatoriosEndpoints(this IEndpointRouteBuilder app)
    {
        var relatorios = app.MapGroup("/api/relatorios").WithTags("Relatorios");

        relatorios.MapGet("/resumo", GetResumoAsync)
                  .WithSummary("Retorna KPIs financeiros do estacionamento")
                  .RequireAuthorization(p => p.RequireRole("Admin"));
    }

    private static async Task<IResult> GetResumoAsync(
        IRelatorioQueries queries,
        CancellationToken ct)
    {
        var resumo = await queries.GetResumoAsync(ct);
        return Results.Ok(resumo);
    }
}
