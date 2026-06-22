using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Shared.Core.Data;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Services;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Infra.Data.EF;

public class ParkingDbContext : DbContext, IUnitOfWork
{
    private readonly ITenantProvider _tenantProvider;

    public ParkingDbContext(DbContextOptions<ParkingDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Vaga> Vagas { get; set; }
    public DbSet<Tarifa> Tarifas { get; set; }
    public DbSet<Movimentacao> Movimentacoes { get; set; }
    public DbSet<Reserva> Reservas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Event>();
        modelBuilder.Ignore<ValidationResult>();
        modelBuilder.UseIdentityAlwaysColumns();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new ValueConverter<DateTime?, DateTime?>(
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v));
                }
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ParkingDbContext).Assembly);

        ApplyTenantFilter<Vaga>(modelBuilder);
        ApplyTenantFilter<Movimentacao>(modelBuilder);
        ApplyTenantFilter<Tarifa>(modelBuilder);
        ApplyTenantFilter<Reserva>(modelBuilder);

        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

        base.OnModelCreating(modelBuilder);
    }

    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, ITrackableEntity, ITenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e =>
            !e.IsDeleted && (_tenantProvider.IsSuperAdmin || e.TenantId == (_tenantProvider.TenantId ?? 0L)));
    }

    public async Task<bool> Commit()
    {
        return await base.SaveChangesAsync() > 0;
    }
}
