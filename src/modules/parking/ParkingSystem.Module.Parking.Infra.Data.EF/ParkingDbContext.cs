using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParkingSystem.Module.Parking.Domain.Entities;
using ParkingSystem.Shared.Core.Data;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Parking.Infra.Data.EF;

public class ParkingDbContext(DbContextOptions<ParkingDbContext> options)
    : DbContext(options), IUnitOfWork
{
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
            if (typeof(ITrackableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ParkingDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, [modelBuilder]);
            }

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

        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

        base.OnModelCreating(modelBuilder);
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : class, ITrackableEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
    }

    public async Task<bool> Commit()
    {
        return await base.SaveChangesAsync() > 0;
    }
}
