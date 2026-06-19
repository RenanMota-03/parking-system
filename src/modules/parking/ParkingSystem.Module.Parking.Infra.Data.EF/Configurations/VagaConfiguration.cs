using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingSystem.Module.Parking.Domain.Entities;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Configurations;

public class VagaConfiguration : IEntityTypeConfiguration<Vaga>
{
    public void Configure(EntityTypeBuilder<Vaga> builder)
    {
        builder.ToTable("vagas");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.Numero).HasColumnName("numero").HasMaxLength(10).IsRequired();
        builder.Property(v => v.TipoVaga).HasColumnType("smallint").HasColumnName("tipo_vaga");
        builder.Property(v => v.Status).HasColumnType("smallint").HasColumnName("status");
        builder.Property(v => v.IsDeleted).HasColumnName("is_deleted");
        builder.Property(v => v.CreatedAt).HasColumnName("created_at");
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(v => v.Numero).IsUnique();
    }
}
