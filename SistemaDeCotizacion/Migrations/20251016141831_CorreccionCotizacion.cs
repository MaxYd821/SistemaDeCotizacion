using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeCotizacion.Migrations
{
    /// <inheritdoc />
    public partial class CorreccionCotizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_Cliente_cliente_id",
                table: "Cotizacion");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_Cliente_cliente_id",
                table: "Cotizacion",
                column: "cliente_id",
                principalTable: "Cliente",
                principalColumn: "cliente_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cotizacion_Cliente_cliente_id",
                table: "Cotizacion");

            migrationBuilder.AddForeignKey(
                name: "FK_Cotizacion_Cliente_cliente_id",
                table: "Cotizacion",
                column: "cliente_id",
                principalTable: "Cliente",
                principalColumn: "cliente_id");
        }
    }
}
