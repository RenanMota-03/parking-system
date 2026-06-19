using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddTarifas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tarifas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tipo_vaga = table.Column<short>(type: "smallint", nullable: false),
                    valor_hora = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_diaria = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    valor_mensal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    vigente_ate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifas", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tarifas");
        }
    }
}
