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
    public class VehiculoTest
    {
        private VehiculoController _controller;
        private AppDBContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDBContext(options);

            // Datos base: un cliente y un vehículo
            var cliente = new Cliente
            {
                cliente_id = 1,
                nombre_cliente = "Juan Perez",
                correo_cliente = "juan@correo.com",
                ruc = "12345678",
                telefono_cliente = "999999999",
                direccion_cliente = "Calle Falsa 123",
                tipo = "Persona Natural",
                fecha_registro_cliente = DateTime.Now
            };
            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            _context.Vehiculos.Add(new Vehiculo
            {
                vehiculo_id = 1,
                modelo = "Corolla",
                marca = "Toyota",
                placa = "ABC123",
                kilometraje = 50000,
                fecha_registro_vehiculo = DateTime.Now,
                cliente_id = 1
            });
            _context.SaveChanges();

            _controller = new VehiculoController(_context)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
        }

        [TestMethod]
        public async Task Mostrar_SinBusqueda_MuestraTodos()
        {
            var result = await _controller.Mostrar();

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var vehiculos = view.Model as System.Collections.Generic.List<Vehiculo>;
            Assert.AreEqual(1, vehiculos.Count);
        }

        [TestMethod]
        public async Task Mostrar_ConBusquedaPorPlaca()
        {
            var result = await _controller.Mostrar("ABC");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var vehiculos = view.Model as System.Collections.Generic.List<Vehiculo>;
            Assert.AreEqual(1, vehiculos.Count);
            Assert.AreEqual("ABC123", vehiculos[0].placa);
        }

        [TestMethod]
        public void Nuevo_Get_MuestraVista()
        {
            var result = _controller.Nuevo();
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        [TestMethod]
        public async Task Nuevo_Post_CreaVehiculoCorrectamente()
        {
            var vehiculo = new Vehiculo
            {
                modelo = "Civic",
                marca = "Honda",
                placa = "XYZ789",
                kilometraje = 20000,
                cliente_id = 1
            };

            var result = await _controller.Nuevo(vehiculo);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);

            var creado = _context.Vehiculos.FirstOrDefault(v => v.placa == "XYZ789");
            Assert.IsNotNull(creado);
        }

        [TestMethod]
        public async Task Nuevo_Post_PlacaDuplicada()
        {
            var vehiculo = new Vehiculo
            {
                modelo = "Yaris",
                marca = "Toyota",
                placa = "ABC123",
                kilometraje = 10000,
                cliente_id = 1
            };

            var result = await _controller.Nuevo(vehiculo);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un vehículo con esa placa.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public async Task Editar_Get_DevuelveVistaConDatos()
        {
            var result = await _controller.Editar(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var model = view.Model as Vehiculo;
            Assert.AreEqual("ABC123", model.placa);
        }

        [TestMethod]
        public async Task Editar_Post_ActualizaVehiculo()
        {
            var vehiculo = new Vehiculo
            {
                vehiculo_id = 1,
                modelo = "Corolla SE",
                marca = "Toyota",
                placa = "ABC123",
                kilometraje = 60000,
                cliente_id = 1
            };

            var result = await _controller.Editar(vehiculo);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actualizado = await _context.Vehiculos.FindAsync(1);
            Assert.AreEqual(60000, actualizado.kilometraje);
            Assert.AreEqual("Corolla SE", actualizado.modelo);
        }

        [TestMethod]
        public async Task Editar_Post_PlacaDuplicada()
        {
            // Agregar otro vehículo para simular duplicado
            _context.Vehiculos.Add(new Vehiculo
            {
                vehiculo_id = 2,
                modelo = "Fit",
                marca = "Honda",
                placa = "XYZ789",
                kilometraje = 40000,
                fecha_registro_vehiculo = DateTime.Now,
                cliente_id = 1
            });
            _context.SaveChanges();

            var vehiculo = new Vehiculo
            {
                vehiculo_id = 1,
                modelo = "Corolla",
                marca = "Toyota",
                placa = "XYZ789", // placa duplicada
                kilometraje = 60000,
                cliente_id = 1
            };

            var result = await _controller.Editar(vehiculo);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un vehículo con esa placa.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public void Eliminar_Get_DevuelveVista()
        {
            var result = _controller.Eliminar(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.IsNotNull(view.Model);
        }

        [TestMethod]
        public void ConfirmacionEliminar_EliminaVehiculo()
        {
            var result = _controller.ConfirmacionEliminar(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var eliminado = _context.Vehiculos.Find(1);
            Assert.IsNull(eliminado);
        }

        [TestMethod]
        public void ConfirmacionEliminar_IdInexistente()
        {
            var result = _controller.ConfirmacionEliminar(99);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Editar_Get_IdInexistente()
        {
            var result = await _controller.Editar(99);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
