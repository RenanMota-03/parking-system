using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingSystem.Module.Identity.Domain.Entities;

namespace ParkingSystem.Module.Identity.Infra.Data.EF.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.Nome).HasColumnName("nome").HasMaxLength(150).IsRequired();
        builder.Property(t => t.CodigoConvite).HasColumnName("codigo_convite").HasMaxLength(8).IsRequired();
        builder.Property(t => t.Ativo).HasColumnName("ativo").IsRequired();
        builder.Property(t => t.IsDeleted).HasColumnName("is_deleted");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(t => t.CodigoConvite).IsUnique();
    }
}
