using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddMovimentacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "movimentacoes",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    vaga_id = table.Column<long>(type: "bigint", nullable: false),
                    placa_veiculo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    data_entrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_saida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valor_total = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    pago = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    forma_pagamento = table.Column<short>(type: "smallint", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimentacoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimentacoes_vagas_vaga_id",
                        column: x => x.vaga_id,
                        principalTable: "vagas",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_movimentacoes_placa_veiculo",
                table: "movimentacoes",
                column: "placa_veiculo");

            migrationBuilder.CreateIndex(
                name: "IX_movimentacoes_vaga_id",
                table: "movimentacoes",
                column: "vaga_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimentacoes");
        }
    }
}
