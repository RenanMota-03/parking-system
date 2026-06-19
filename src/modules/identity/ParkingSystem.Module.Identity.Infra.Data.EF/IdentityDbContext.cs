using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Shared.Core.Data;
using ParkingSystem.Shared.Core.DomainObjects;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Identity.Infra.Data.EF;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Event>();
        modelBuilder.Ignore<ValidationResult>();
        modelBuilder.UseIdentityAlwaysColumns();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITrackableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(IdentityDbContext)
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
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

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
