using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingSystem.Module.Parking.Domain.Entities;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Configurations;

public class ReservaConfiguration : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> builder)
    {
        builder.ToTable("reservas");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.VagaId).HasColumnName("vaga_id");
        builder.Property(r => r.UsuarioId).HasColumnName("usuario_id").HasMaxLength(100).IsRequired();
        builder.Property(r => r.DataAgendada).HasColumnName("data_agendada");
        builder.Property(r => r.DataLimite).HasColumnName("data_limite");
        builder.Property(r => r.Status).HasColumnType("smallint").HasColumnName("status");
        builder.Property(r => r.IsDeleted).HasColumnName("is_deleted");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(r => r.Vaga)
               .WithMany()
               .HasForeignKey(r => r.VagaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.VagaId);
        builder.HasIndex(r => r.UsuarioId);
    }
}
