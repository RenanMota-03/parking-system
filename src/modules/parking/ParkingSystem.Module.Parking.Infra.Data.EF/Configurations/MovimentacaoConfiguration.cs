using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ParkingSystem.Module.Parking.Domain.Entities;

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Configurations;

public class MovimentacaoConfiguration : IEntityTypeConfiguration<Movimentacao>
{
    public void Configure(EntityTypeBuilder<Movimentacao> builder)
    {
        builder.ToTable("movimentacoes");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(m => m.VagaId).HasColumnName("vaga_id");
        builder.Property(m => m.PlacaVeiculo).HasColumnName("placa_veiculo").HasMaxLength(8).IsRequired();
        builder.Property(m => m.DataEntrada).HasColumnName("data_entrada");
        builder.Property(m => m.DataSaida).HasColumnName("data_saida");
        builder.Property(m => m.ValorTotal).HasColumnType("numeric(18,2)").HasColumnName("valor_total");
        builder.Property(m => m.Pago).HasColumnName("pago").HasDefaultValue(false);
        builder.Property(m => m.FormaPagamento).HasColumnType("smallint").HasColumnName("forma_pagamento");
        builder.Property(m => m.IsDeleted).HasColumnName("is_deleted");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(m => m.Vaga)
               .WithMany()
               .HasForeignKey(m => m.VagaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.PlacaVeiculo);
        builder.HasIndex(m => m.VagaId);
    }
}
