using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reservas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    vaga_id = table.Column<long>(type: "bigint", nullable: false),
                    usuario_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data_agendada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_limite = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservas", x => x.id);
                    table.ForeignKey(
                        name: "FK_reservas_vagas_vaga_id",
                        column: x => x.vaga_id,
                        principalTable: "vagas",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_reservas_usuario_id",
                table: "reservas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_reservas_vaga_id",
                table: "reservas",
                column: "vaga_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservas");
        }
    }
}
