using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingSystem.Module.Parking.Domain.Entities;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Configurations;

public class TarifaConfiguration : IEntityTypeConfiguration<Tarifa>
{
    public void Configure(EntityTypeBuilder<Tarifa> builder)
    {
        builder.ToTable("tarifas");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(t => t.TipoVaga).HasColumnType("smallint").HasColumnName("tipo_vaga");
        builder.Property(t => t.ValorHora).HasColumnType("numeric(18,2)").HasColumnName("valor_hora");
        builder.Property(t => t.ValorDiaria).HasColumnType("numeric(18,2)").HasColumnName("valor_diaria");
        builder.Property(t => t.ValorMensal).HasColumnType("numeric(18,2)").HasColumnName("valor_mensal");
        builder.Property(t => t.VigenteAte).HasColumnName("vigente_ate");
        builder.Property(t => t.IsDeleted).HasColumnName("is_deleted");
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.UpdatedAt).HasColumnName("updated_at");
    }
}
