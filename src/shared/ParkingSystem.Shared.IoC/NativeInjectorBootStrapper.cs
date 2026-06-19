using Microsoft.Extensions.DependencyInjection;
using ParkingSystem.Module.Identity.Application.Usuario.Commands;
using ParkingSystem.Module.Identity.Application.Usuario.Queries;
using ParkingSystem.Module.Identity.Domain.Interfaces;
using ParkingSystem.Module.Identity.Infra.Data.EF.Repository;
using ParkingSystem.Module.Parking.Application.Movimentacao.Commands;
using ParkingSystem.Module.Parking.Application.Movimentacao.Queries;
using ParkingSystem.Module.Parking.Application.Relatorio.Queries;
using ParkingSystem.Module.Parking.Application.Reserva.Commands;
using ParkingSystem.Module.Parking.Application.Reserva.Queries;
using ParkingSystem.Module.Parking.Application.Tarifa.Commands;
using ParkingSystem.Module.Parking.Application.Tarifa.Queries;
using ParkingSystem.Module.Parking.Application.Tarifa.Services;
using ParkingSystem.Module.Parking.Application.Vaga.Commands;
using ParkingSystem.Module.Parking.Application.Vaga.Queries;
using ParkingSystem.Module.Parking.Domain.Interfaces;
using ParkingSystem.Module.Parking.Infra.Data.EF.Repository;
using ParkingSystem.Shared.Core.Messaging;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Shared.IoC;

public static class NativeInjectorBootStrapper
{
    public static void RegisterServices(IServiceCollection services)
    {
        // ── Identity repositories & queries ──────────────────────────────────
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUsuarioQueries, UsuarioQueries>();
        services.AddScoped<ICommandHandler<RegistrarUsuarioCommand, ValidationResult>, RegistrarUsuarioCommandHandler>();

        // ── Parking repositories ──────────────────────────────────────────────
        services.AddScoped<IVagaRepository, VagaRepository>();
        services.AddScoped<ITarifaRepository, TarifaRepository>();
        services.AddScoped<IMovimentacaoRepository, MovimentacaoRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();

        // ── Parking services ──────────────────────────────────────────────────
        services.AddScoped<ITarifaService, TarifaService>();

        // ── Parking queries ───────────────────────────────────────────────────
        services.AddScoped<IVagaQueries, VagaQueries>();
        services.AddScoped<ITarifaQueries, TarifaQueries>();
        services.AddScoped<IMovimentacaoQueries, MovimentacaoQueries>();
        services.AddScoped<IReservaQueries, ReservaQueries>();
        services.AddScoped<IRelatorioQueries, RelatorioQueries>();

        // ── Parking command handlers ──────────────────────────────────────────
        services.AddScoped<ICommandHandler<CadastrarVagaCommand, ValidationResult>, CadastrarVagaCommandHandler>();
        services.AddScoped<ICommandHandler<AlterarStatusVagaCommand, ValidationResult>, AlterarStatusVagaCommandHandler>();
        services.AddScoped<ICommandHandler<CadastrarTarifaCommand, ValidationResult>, CadastrarTarifaCommandHandler>();
        services.AddScoped<ICommandHandler<AtualizarTarifaCommand, ValidationResult>, AtualizarTarifaCommandHandler>();
        services.AddScoped<ICommandHandler<RegistrarEntradaCommand, ValidationResult>, RegistrarEntradaCommandHandler>();
        services.AddScoped<ICommandHandler<RegistrarSaidaCommand, ValidationResult>, RegistrarSaidaCommandHandler>();
        services.AddScoped<ICommandHandler<PagarCommand, ValidationResult>, PagarCommandHandler>();
        services.AddScoped<ICommandHandler<CriarReservaCommand, ValidationResult>, CriarReservaCommandHandler>();
        services.AddScoped<ICommandHandler<CancelarReservaCommand, ValidationResult>, CancelarReservaCommandHandler>();
    }
}
