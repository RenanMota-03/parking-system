using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ParkingSystem.Module.Identity.Domain.Entities;
using ParkingSystem.Shared.Core.Data;
using ParkingSystem.Shared.Core.Messages;
using ParkingSystem.Shared.Core.Services;
using ParkingSystem.Shared.Core.Validation;

namespace ParkingSystem.Module.Identity.Infra.Data.EF;

public class IdentityDbContext : DbContext, IUnitOfWork
{
    private readonly ITenantProvider _tenantProvider;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

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
            }
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        modelBuilder.Entity<Usuario>().HasQueryFilter(u =>
            !u.IsDeleted && (_tenantProvider.IsSuperAdmin || u.TenantId == _tenantProvider.TenantId));

        modelBuilder.Entity<Tenant>().HasQueryFilter(t => !t.IsDeleted);

        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

        base.OnModelCreating(modelBuilder);
    }

    public async Task<bool> Commit()
    {
        return await base.SaveChangesAsync() > 0;
    }
}
