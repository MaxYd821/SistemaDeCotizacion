using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeCotizacion.Migrations
{
    /// <inheritdoc />
    public partial class Migracion2_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "stock",
                table: "Repuesto",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stock",
                table: "Repuesto");
        }
    }
}
