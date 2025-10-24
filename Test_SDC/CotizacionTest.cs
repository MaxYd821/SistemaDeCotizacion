using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SistemaDeCotizacion.Controllers;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test_SDC
{
    [TestClass]
    public class CotizacionTest
    {
        private AppDBContext _context;
        private CotizacionController _controller;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new AppDBContext(options);

            // Datos base
            var cliente = new Cliente { cliente_id = 1, nombre_cliente = "Juan Pérez", correo_cliente = "juan1@gmail.com", 
                ruc = "12345678", fecha_registro_cliente =DateTime.Now, telefono_cliente = "912345678", direccion_cliente = "dir", 
                tipo = "Persona Natural" };
            var vehiculo = new Vehiculo { vehiculo_id = 1, marca = "Toyota", modelo = "Corolla", placa = "ABC-123", cliente_id = 1 };
            var servicio = new Servicio { servicio_id = 1, nombre_servicio = "Cambio de aceite", precio = 100 };
            var repuesto = new Repuesto { repuesto_id = 1, codigo_rep = "C1", medida_rep = "Unidad", descripcion = "Filtro de aceite", precio_und = 50, stock = 10 };

            _context.Clientes.Add(cliente);
            _context.Vehiculos.Add(vehiculo);
            _context.Servicios.Add(servicio);
            _context.Repuestos.Add(repuesto);
            _context.SaveChanges();

            _controller = new CotizacionController(_context);

            // Crear user con claim Name (usado por tu controlador)
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Trabajador1"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1")
            };
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };

            // Asignar HttpContext al ControllerContext y usarlo también para TempData
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>()
            );
        }

        [TestMethod]
        public async Task NuevoCotizacion_CreacionExitosa()
        {
            // Arrange
            var model = new CotizacionVM
            {
                CotizacionId = 1,
                ClienteId = 1,
                VehiculoId = 1,
                formaPago = "Contado",
                tiempoEntrega = 2,
                estado_cotizacion = "Pendiente",
                trabajador = "Trabajador1",
                ServiciosSeleccionados = new List<ServicioSeleccionadoVM>
                {
                    new ServicioSeleccionadoVM { ServicioId = 1 }
                },
                RepuestosSeleccionados = new List<RepuestoSeleccionadoVM>
                {
                    new RepuestoSeleccionadoVM { RepuestoId = 1, Cantidad = 2 }
                }
            };

            // Act
            var result = await _controller.Nuevo(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);
        }

        [TestMethod]
        public async Task NuevoCotizacion_RepuestoSinStock()
        {
            // Arrange
            var rep = _context.Repuestos.First();
            rep.stock = 1;
            _context.SaveChanges();

            var model = new CotizacionVM
            {
                ClienteId = 1,
                VehiculoId = 1,
                formaPago = "Contado",
                tiempoEntrega = 2,
                estado_cotizacion = "Pendiente",
                trabajador = "Trabajador1",
                ServiciosSeleccionados = new List<ServicioSeleccionadoVM>
                {
                    new ServicioSeleccionadoVM { ServicioId = 1 }
                },
                RepuestosSeleccionados = new List<RepuestoSeleccionadoVM>
                {
                    new RepuestoSeleccionadoVM { RepuestoId = rep.repuesto_id, Cantidad = 5 }
                }
            };

            // Act
            var result = await _controller.Nuevo(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual($"El repuesto '{rep.descripcion}' no tiene suficiente stock. Disponible: {rep.stock}", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public async Task EditarCotizacion_ActualizaDatos()
        {
            // Arrange: Crear una cotización inicial
            var cot = new Cotizacion
            {
                cotizacion_id = 1,
                cliente_id = 1,
                formaPago = "Contado",
                tiempoEntrega = 2,
                estado_cotizacion = "Pendiente",
                trabajador = "Trabajador1",
                costo_servicio_total = 100,
                costo_repuesto_total = 50
            };
            _context.Cotizaciones.Add(cot);
            _context.SaveChanges();

            var model = new CotizacionVM
            {
                ClienteId = 1,
                VehiculoId = 1,
                formaPago = "Crédito",
                tiempoEntrega = 3,
                estado_cotizacion = "Aprobado",
                ServiciosSeleccionados = new List<ServicioSeleccionadoVM>
                {
                    new ServicioSeleccionadoVM { ServicioId = 1 }
                },
                RepuestosSeleccionados = new List<RepuestoSeleccionadoVM>
                {
                    new RepuestoSeleccionadoVM { RepuestoId = 1, Cantidad = 1 }
                }
            };

            // Act
            var result = await _controller.Editar(1, model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actualizado = await _context.Cotizaciones.FindAsync(1);
            Assert.AreEqual("Aprobado", actualizado.estado_cotizacion);
            Assert.AreEqual("Crédito", actualizado.formaPago);
        }

        [TestMethod]
        public void ConfirmacionEliminar_EliminaCotizacionYDevuelveStock()
        {
            // Arrange
            var cot = new Cotizacion
            {
                cotizacion_id = 1,
                cliente_id = 1,
                formaPago = "Contado",
                tiempoEntrega = 2,
                estado_cotizacion = "Pendiente",
                trabajador = "Trabajador1",
                costo_servicio_total = 100,
                costo_repuesto_total = 50,
                repuestos = new List<DetalleRepuesto>
                {
                    new DetalleRepuesto { repuesto_id = 1, cantidad_rep = 2, valor_venta = 100 }
                }
            };

            _context.Cotizaciones.Add(cot);
            _context.SaveChanges();

            var stockInicial = _context.Repuestos.First().stock;

            // Act
            var result = _controller.ConfirmacionEliminar(1) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Mostrar", result.ActionName);
            Assert.AreEqual(stockInicial + 2, _context.Repuestos.First().stock);
            Assert.AreEqual(0, _context.Cotizaciones.Count());
        }

        [TestMethod]
        public async Task Mostrar_FiltraPorEstado()
        {
            // Arrange
            _context.Cotizaciones.AddRange(
                new Cotizacion {
                    cotizacion_id = 1,
                    cliente_id = 1,
                    formaPago = "Contado",
                    tiempoEntrega = 2,
                    estado_cotizacion = "Aprobado",
                    trabajador = "Trabajador1",
                    costo_servicio_total = 100,
                    costo_repuesto_total = 50,
                    fecha_cotizacion = DateTime.Now },
                new Cotizacion { 
                    cotizacion_id = 2, 
                    cliente_id = 1,
                    formaPago = "Contado",
                    tiempoEntrega = 2,
                    estado_cotizacion = "Pendiente",
                    trabajador = "Trabajador1",
                    costo_servicio_total = 100,
                    costo_repuesto_total = 50,
                    fecha_cotizacion = DateTime.Now }
            );
            _context.SaveChanges();

            // Act
            var result = await _controller.Mostrar(null, null, null, "Aprobado");

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var model = view.Model as List<Cotizacion>;

            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Aprobado", model.First().estado_cotizacion);
        }

        [TestMethod]
        public void ConfirmacionEliminar_CotizacionConIngreso_NoDebeEliminarse()
        {
            // Arrange
            var cot = new Cotizacion { 
                cotizacion_id = 10, 
                cliente_id = 1,
                formaPago = "Contado",
                estado_cotizacion = "Pendiente",
                trabajador = "Trabajador1",
                costo_servicio_total = 100,
                costo_repuesto_total = 50,
                fecha_cotizacion = DateTime.Now
            };
            _context.Cotizaciones.Add(cot);
            _context.Ingresos.Add(new Ingreso
            {
                ingreso_id = 1,
                costo_ingreso = 150,
                fecha_ingreso = DateTime.Now,
                fecha_registro_ingreso = DateTime.Now,
                tipo_ingreso = "Por Cotización",
                detalle_ingreso = "Ingreso generado a partir de la cotización ID: #10"
            });
            _context.SaveChanges();

            // Act
            var result = _controller.ConfirmacionEliminar(10) as RedirectToActionResult;

            // Assert
            Assert.AreEqual("Mostrar", result.ActionName);
            Assert.IsTrue(_controller.TempData.ContainsKey("error"));
            StringAssert.Contains(_controller.TempData["error"].ToString(), "No se puede eliminar");
        }
    }
}
