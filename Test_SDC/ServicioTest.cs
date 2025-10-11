using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaDeCotizacion.Controllers;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Test_SDC
{
    [TestClass]
    public class ServicioTest
    {
        private ServicioController _controller;
        private AppDBContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDBContext(options);

            // Agregamos datos iniciales
            _context.Servicios.Add(new Servicio
            {
                servicio_id = 1,
                nombre_servicio = "Lavado general",
                precio = 25.5,
                fecha_registro_servicio = DateTime.Now
            });
            _context.SaveChanges();

            _controller = new ServicioController(_context)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
        }

        // ---------------------------
        // MÉTODOS: MOSTRAR
        // ---------------------------
        [TestMethod]
        public async Task Mostrar_SinBusqueda_MuestraTodos()
        {
            var result = await _controller.Mostrar();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var servicios = view.Model as System.Collections.Generic.List<Servicio>;
            Assert.AreEqual(1, servicios.Count);
        }

        [TestMethod]
        public async Task Mostrar_ConBusquedaPorNombre()
        {
            var result = await _controller.Mostrar("Lavado");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var servicios = view.Model as System.Collections.Generic.List<Servicio>;
            Assert.AreEqual(1, servicios.Count);
            Assert.AreEqual("Lavado general", servicios[0].nombre_servicio);
        }

        // ---------------------------
        // MÉTODOS: NUEVO
        // ---------------------------
        [TestMethod]
        public void Nuevo_Get_MuestraVista()
        {
            var result = _controller.Nuevo();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Nuevo_Post_CreaServicioCorrectamente()
        {
            var servicio = new Servicio
            {
                nombre_servicio = "Cambio de aceite",
                precio = 50.0
            };

            var result = await _controller.Nuevo(servicio);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);

            var creado = _context.Servicios.FirstOrDefault(s => s.nombre_servicio == "Cambio de aceite");
            Assert.IsNotNull(creado);
            Assert.AreEqual(50.0, creado.precio);
        }

        [TestMethod]
        public async Task Nuevo_Post_NombreDuplicado()
        {
            var servicio = new Servicio
            {
                nombre_servicio = "Lavado general",
                precio = 30.0
            };

            var result = await _controller.Nuevo(servicio);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un servicio con este nombre.", view.ViewData["mensaje"]);
        }

        // ---------------------------
        // MÉTODOS: EDITAR
        // ---------------------------
        [TestMethod]
        public async Task Editar_Get_DevuelveVistaConDatos()
        {
            var result = await _controller.Editar(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var model = view.Model as Servicio;
            Assert.AreEqual("Lavado general", model.nombre_servicio);
        }

        [TestMethod]
        public async Task Editar_Post_ActualizaServicio()
        {
            var servicio = new Servicio
            {
                servicio_id = 1,
                nombre_servicio = "Lavado premium",
                precio = 40.0
            };

            var result = await _controller.Editar(servicio);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actualizado = await _context.Servicios.FindAsync(1);
            Assert.AreEqual("Lavado premium", actualizado.nombre_servicio);
            Assert.AreEqual(40.0, actualizado.precio);
        }

        [TestMethod]
        public async Task Editar_Post_NombreDuplicado()
        {
            // Agregamos un segundo servicio para simular duplicado
            _context.Servicios.Add(new Servicio
            {
                servicio_id = 2,
                nombre_servicio = "Cambio de llantas",
                precio = 60.0,
                fecha_registro_servicio = DateTime.Now
            });
            _context.SaveChanges();

            var servicio = new Servicio
            {
                servicio_id = 1,
                nombre_servicio = "Cambio de llantas", // duplicado
                precio = 35.0
            };

            var result = await _controller.Editar(servicio);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un servicio con este nombre.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public async Task Editar_Get_IdInexistente()
        {
            var result = await _controller.Editar(99);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ---------------------------
        // MÉTODOS: ELIMINAR
        // ---------------------------
        [TestMethod]
        public void Eliminar_Get_DevuelveVista()
        {
            var result = _controller.Eliminar(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.IsNotNull(view.Model);
        }

        [TestMethod]
        public void ConfirmacionEliminar_EliminaServicio()
        {
            var result = _controller.ConfirmacionEliminar(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var eliminado = _context.Servicios.Find(1);
            Assert.IsNull(eliminado);
        }

        [TestMethod]
        public void ConfirmacionEliminar_IdInexistente()
        {
            var result = _controller.ConfirmacionEliminar(99);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
