using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SistemaDeCotizacion.Controllers;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Test_SDC
{
    [TestClass]
    public class IngresoTest
    {
        private AppDBContext _context;
        private IngresoController _controller;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDBContext(options);

            // Datos iniciales
            var usuario = new Usuario
            {
                usuario_id = 1,
                nombre = "Max",
                apellido = "Ramirez",
                correo = "max@correo.com",
                dni = "12345678",
                estado = "Activo",
                num_cel = "987654321",
                password = "12345"
            };

            var cotizacion = new Cotizacion
            {
                cotizacion_id = 1,
                costo_servicio_total = 100,
                costo_repuesto_total = 50,
                formaPago = "Efectivo",
                trabajador = "Max Ramirez",
                estado_cotizacion = "Aprobado"
            };

            _context.Usuarios.Add(usuario);
            _context.Cotizaciones.Add(cotizacion);
            _context.SaveChanges();

            _controller = new IngresoController(_context);
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Simular usuario logueado
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [TestMethod]
        public void NuevoIngreso_CreacionExitosa()
        {
            var ingreso = new Ingreso
            {
                fecha_ingreso = DateTime.Now,
                tipo_ingreso = "Por Cotización",
                detalle_ingreso = "Prueba"
            };

            var result = _controller.Nuevo(ingreso, 1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);

            var creado = _context.Ingresos.FirstOrDefault();
            Assert.IsNotNull(creado);
            Assert.AreEqual(150, creado.costo_ingreso);
            Assert.AreEqual("Ingresado", _context.Cotizaciones.First().estado_cotizacion);
        }

        [TestMethod]
        public void NuevoIngreso_CotizacionNoExiste()
        {
            var ingreso = new Ingreso
            {
                fecha_ingreso = DateTime.Now,
                tipo_ingreso = "Por Cotización"
            };

            var result = _controller.Nuevo(ingreso, 999);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.IsTrue(view.ViewData.ModelState.ErrorCount > 0);
        }

        [TestMethod]
        public async Task Mostrar_FiltraPorBusquedaYFecha()
        {
            // Arrange
            _context.Ingresos.AddRange(
                new Ingreso
                {
                    ingreso_id = 1,
                    usuario_id = 1,
                    tipo_ingreso = "Por Cotización",
                    fecha_ingreso = new DateTime(2024, 5, 15),
                    detalle_ingreso = "Ingreso 1"
                },
                new Ingreso
                {
                    ingreso_id = 2,
                    usuario_id = 1,
                    tipo_ingreso = "Venta",
                    fecha_ingreso = new DateTime(2024, 6, 10),
                    detalle_ingreso = "Ingreso 2"
                }
            );
            _context.SaveChanges();

            // Act
            var result = await _controller.Mostrar("Por Cotización", 5, 2024);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var model = view.Model as List<Ingreso>;

            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Por Cotización", model.First().tipo_ingreso);
        }

        [TestMethod]
        public void EditarIngreso_ActualizaCorrectamente()
        {
            var ingreso = new Ingreso
            {
                ingreso_id = 1,
                fecha_ingreso = DateTime.Now,
                tipo_ingreso = "Por Cotización",
                usuario_id = 1,
                detalle_ingreso = "Ingreso original",
                costo_ingreso = 100
            };
            _context.Ingresos.Add(ingreso);
            _context.SaveChanges();

            var ingresoEditado = new Ingreso
            {
                ingreso_id = 1,
                fecha_ingreso = DateTime.Now.AddDays(1),
                tipo_ingreso = "Venta"
            };

            var result = _controller.Editar(ingresoEditado, 1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var actualizado = _context.Ingresos.Find(1);
            Assert.AreEqual("Venta", actualizado.tipo_ingreso);
            Assert.AreEqual(150, actualizado.costo_ingreso);
        }

        [TestMethod]
        public void EditarIngreso_CotizacionNoExiste()
        {
            var ingreso = new Ingreso
            {
                ingreso_id = 1,
                fecha_ingreso = DateTime.Now,
                tipo_ingreso = "Por Cotización",
                detalle_ingreso = "Prueba",
                costo_ingreso = 0
            };

            _context.Ingresos.Add(ingreso);
            _context.SaveChanges();

            var result = _controller.Editar(ingreso, 999);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("No se encontró la cotización seleccionada.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public void EliminarIngreso_MuestraVistaCorrecta()
        {
            var ingreso = new Ingreso
            {
                ingreso_id = 1,
                usuario_id = 1,
                tipo_ingreso = "Por Cotización",
                detalle_ingreso = "Ingreso generado a partir de la cotización ID: #1"
            };
            _context.Ingresos.Add(ingreso);
            _context.SaveChanges();

            var result = _controller.Eliminar(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.IsNotNull(view.Model);
        }

        [TestMethod]
        public void ConfirmacionEliminar_EliminaYActualizaCotizacion()
        {
            var ingreso = new Ingreso
            {
                ingreso_id = 1,
                usuario_id = 1,
                tipo_ingreso = "Por Cotización",
                detalle_ingreso = "Ingreso generado a partir de la cotización ID: #1"
            };
            _context.Ingresos.Add(ingreso);
            _context.SaveChanges();

            var result = _controller.ConfirmacionEliminar(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            Assert.AreEqual(0, _context.Ingresos.Count());
            Assert.AreEqual("Aprobado", _context.Cotizaciones.First().estado_cotizacion);
        }
    }
}
