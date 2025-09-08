using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeCotizacion.Migrations
{
    /// <inheritdoc />
    public partial class Migration1TodoCam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repuesto",
                columns: table => new
                {
                    repuesto_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigo_rep = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    medida_rep = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    precio_und = table.Column<double>(type: "float(8)", precision: 8, scale: 2, nullable: false),
                    fecha_registro_repuesto = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repuesto", x => x.repuesto_id);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rol_nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    rol_descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "Servicio",
                columns: table => new
                {
                    servicio_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_servicio = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    precio = table.Column<double>(type: "float(8)", precision: 8, scale: 2, nullable: false),
                    fecha_registro_servicio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicio", x => x.servicio_id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    usuario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    apellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    num_cel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    dni = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    correo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    rol_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.usuario_id);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_rol_id",
                        column: x => x.rol_id,
                        principalTable: "Rol",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cliente",
                columns: table => new
                {
                    cliente_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre_cliente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    correo_cliente = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ruc = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    fecha_registro_cliente = table.Column<DateTime>(type: "datetime2", nullable: false),
                    telefono_cliente = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    direccion_cliente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cliente", x => x.cliente_id);
                    table.ForeignKey(
                        name: "FK_Cliente_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id");
                });

            migrationBuilder.CreateTable(
                name: "Ingreso",
                columns: table => new
                {
                    ingreso_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    costo_ingreso = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    fecha_ingreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    fecha_registro_ingreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tipo_ingreso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    detalle_ingreso = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    usuario_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingreso", x => x.ingreso_id);
                    table.ForeignKey(
                        name: "FK_Ingreso_Usuario_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "Usuario",
                        principalColumn: "usuario_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cotizacion",
                columns: table => new
                {
                    cotizacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    costo_servicio_total = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    costo_repuesto_total = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    formaPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    estado_cotizacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fecha_cotizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    tiempoEntrega = table.Column<int>(type: "int", nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizacion", x => x.cotizacion_id);
                    table.ForeignKey(
                        name: "FK_Cotizacion_Cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Cliente",
                        principalColumn: "cliente_id");
                });

            migrationBuilder.CreateTable(
                name: "Vehiculo",
                columns: table => new
                {
                    vehiculo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    placa = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    fecha_registro_vehiculo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    kilometraje = table.Column<int>(type: "int", nullable: false),
                    cliente_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculo", x => x.vehiculo_id);
                    table.ForeignKey(
                        name: "FK_Vehiculo_Cliente_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "Cliente",
                        principalColumn: "cliente_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleRepuesto",
                columns: table => new
                {
                    detalleRepuesto_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cantidad_rep = table.Column<int>(type: "int", nullable: false),
                    valor_venta = table.Column<double>(type: "float(8)", precision: 8, scale: 2, nullable: false),
                    repuesto_id = table.Column<int>(type: "int", nullable: false),
                    cotizacion_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleRepuesto", x => x.detalleRepuesto_id);
                    table.ForeignKey(
                        name: "FK_DetalleRepuesto_Cotizacion_cotizacion_id",
                        column: x => x.cotizacion_id,
                        principalTable: "Cotizacion",
                        principalColumn: "cotizacion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleRepuesto_Repuesto_repuesto_id",
                        column: x => x.repuesto_id,
                        principalTable: "Repuesto",
                        principalColumn: "repuesto_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleServicio",
                columns: table => new
                {
                    detalleServicio_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    servicio_id = table.Column<int>(type: "int", nullable: false),
                    cotizacion_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleServicio", x => x.detalleServicio_id);
                    table.ForeignKey(
                        name: "FK_DetalleServicio_Cotizacion_cotizacion_id",
                        column: x => x.cotizacion_id,
                        principalTable: "Cotizacion",
                        principalColumn: "cotizacion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetalleServicio_Servicio_servicio_id",
                        column: x => x.servicio_id,
                        principalTable: "Servicio",
                        principalColumn: "servicio_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_correo_cliente",
                table: "Cliente",
                column: "correo_cliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_ruc",
                table: "Cliente",
                column: "ruc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_usuario_id",
                table: "Cliente",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizacion_cliente_id",
                table: "Cotizacion",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRepuesto_cotizacion_id",
                table: "DetalleRepuesto",
                column: "cotizacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRepuesto_repuesto_id",
                table: "DetalleRepuesto",
                column: "repuesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleServicio_cotizacion_id",
                table: "DetalleServicio",
                column: "cotizacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleServicio_servicio_id",
                table: "DetalleServicio",
                column: "servicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ingreso_usuario_id",
                table: "Ingreso",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_correo",
                table: "Usuario",
                column: "correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_dni",
                table: "Usuario",
                column: "dni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_rol_id",
                table: "Usuario",
                column: "rol_id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_cliente_id",
                table: "Vehiculo",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculo_placa",
                table: "Vehiculo",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleRepuesto");

            migrationBuilder.DropTable(
                name: "DetalleServicio");

            migrationBuilder.DropTable(
                name: "Ingreso");

            migrationBuilder.DropTable(
                name: "Vehiculo");

            migrationBuilder.DropTable(
                name: "Repuesto");

            migrationBuilder.DropTable(
                name: "Cotizacion");

            migrationBuilder.DropTable(
                name: "Servicio");

            migrationBuilder.DropTable(
                name: "Cliente");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
