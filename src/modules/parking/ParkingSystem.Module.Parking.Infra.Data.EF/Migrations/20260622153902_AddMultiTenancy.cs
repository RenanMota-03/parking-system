using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingSystem.Module.Parking.Infra.Data.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "vagas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "tarifas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "reservas",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "tenant_id",
                table: "movimentacoes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "vagas");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "tarifas");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "movimentacoes");
        }
    }
}
