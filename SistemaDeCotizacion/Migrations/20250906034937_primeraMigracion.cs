using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDeCotizacion.Migrations
{
    /// <inheritdoc />
    public partial class primeraMigracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Repuestos",
                columns: table => new
                {
                    idRepuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    codigoRepuesto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    medidaRepuesto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    precioRepuesto = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    fechaRegistroRepuesto = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repuestos", x => x.idRepuesto);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rol_nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rol_descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.rol_id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    idServicio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombreServicio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    precioServicio = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    fechaRegistroServicio = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.idServicio);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    idUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    apellido = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    dni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaRegistro = table.Column<DateOnly>(type: "date", nullable: false),
                    fechaNacimineto = table.Column<DateOnly>(type: "date", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    idRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.idUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_idRol",
                        column: x => x.idRol,
                        principalTable: "Roles",
                        principalColumn: "rol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    idCliente = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ruc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    correoCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fechaRegistroCliente = table.Column<DateOnly>(type: "date", nullable: false),
                    telefonoCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    direccionCliente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    idUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.idCliente);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_idUsuario",
                        column: x => x.idUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "idUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingresos",
                columns: table => new
                {
                    idIngreso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    costoIngreso = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    fechaIngreso = table.Column<DateOnly>(type: "date", nullable: false),
                    fechaRegistroIngreso = table.Column<DateOnly>(type: "date", nullable: false),
                    tipoIngreso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    detalleIngreso = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    idUsuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingresos", x => x.idIngreso);
                    table.ForeignKey(
                        name: "FK_Ingresos_Usuarios_idUsuario",
                        column: x => x.idUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "idUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cotizaciones",
                columns: table => new
                {
                    idCotizacion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    costoServicioTotal = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    costoRepuestosTotal = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    formaPago = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    estadoCotizacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tiempoEntrega = table.Column<int>(type: "int", nullable: false),
                    idCliente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotizaciones", x => x.idCotizacion);
                    table.ForeignKey(
                        name: "FK_Cotizaciones_Clientes_idCliente",
                        column: x => x.idCliente,
                        principalTable: "Clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    idVehiculo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    modelo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    marca = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    placa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    kilometraje = table.Column<int>(type: "int", nullable: false),
                    idCliente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.idVehiculo);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Clientes_idCliente",
                        column: x => x.idCliente,
                        principalTable: "Clientes",
                        principalColumn: "idCliente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleRepuestos",
                columns: table => new
                {
                    idDetalleRepuesto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaDetalleRepuesto = table.Column<DateOnly>(type: "date", nullable: false),
                    cantidadRepuesto = table.Column<int>(type: "int", nullable: false),
                    valorVenta = table.Column<double>(type: "float", nullable: false),
                    idRepuesto = table.Column<int>(type: "int", nullable: false),
                    idCotizacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleRepuestos", x => x.idDetalleRepuesto);
                    table.ForeignKey(
                        name: "FK_DetalleRepuestos_Cotizaciones_idCotizacion",
                        column: x => x.idCotizacion,
                        principalTable: "Cotizaciones",
                        principalColumn: "idCotizacion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleRepuestos_Repuestos_idRepuesto",
                        column: x => x.idRepuesto,
                        principalTable: "Repuestos",
                        principalColumn: "idRepuesto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetalleServicios",
                columns: table => new
                {
                    idDetalleServicio = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fechaServicio = table.Column<DateOnly>(type: "date", nullable: false),
                    idServicio = table.Column<int>(type: "int", nullable: false),
                    idCotizacion = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleServicios", x => x.idDetalleServicio);
                    table.ForeignKey(
                        name: "FK_DetalleServicios_Cotizaciones_idCotizacion",
                        column: x => x.idCotizacion,
                        principalTable: "Cotizaciones",
                        principalColumn: "idCotizacion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleServicios_Servicios_idServicio",
                        column: x => x.idServicio,
                        principalTable: "Servicios",
                        principalColumn: "idServicio",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_idUsuario",
                table: "Clientes",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Cotizaciones_idCliente",
                table: "Cotizaciones",
                column: "idCliente");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRepuestos_idCotizacion",
                table: "DetalleRepuestos",
                column: "idCotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleRepuestos_idRepuesto",
                table: "DetalleRepuestos",
                column: "idRepuesto");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleServicios_idCotizacion",
                table: "DetalleServicios",
                column: "idCotizacion");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleServicios_idServicio",
                table: "DetalleServicios",
                column: "idServicio");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_idUsuario",
                table: "Ingresos",
                column: "idUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_idRol",
                table: "Usuarios",
                column: "idRol");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_idCliente",
                table: "Vehiculos",
                column: "idCliente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleRepuestos");

            migrationBuilder.DropTable(
                name: "DetalleServicios");

            migrationBuilder.DropTable(
                name: "Ingresos");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Repuestos");

            migrationBuilder.DropTable(
                name: "Cotizaciones");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
