using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SistemaDeCotizacion.Controllers;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace Test_SDC
{
    [TestClass]
    public class RepuestoTest
    {
        private RepuestoController _controller;
        private AppDBContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDBContext(options);

            _context.Repuestos.Add(new Repuesto
            {
                repuesto_id = 1,
                codigo_rep = "REP001",
                descripcion = "Filtro de aceite",
                stock = 10,
                medida_rep = "unidad",
                precio_und = 25.0,
                fecha_registro_repuesto = DateTime.Now
            });

            _context.SaveChanges();

            _controller = new RepuestoController(_context);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );
        }

        [TestMethod]
        public async Task NuevoRepuesto_CreacionExitosa()
        {
            var repuesto = new Repuesto
            {
                codigo_rep = "REP002",
                descripcion = "Bujía",
                stock = 5,
                medida_rep = "unidad",
                precio_und = 15.5
            };

            var result = await _controller.Nuevo(repuesto);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);

            var creado = _context.Repuestos.FirstOrDefault(r => r.codigo_rep == "REP002");
            Assert.IsNotNull(creado);
        }

        [TestMethod]
        public async Task NuevoRepuesto_CodigoDuplicado()
        {
            var repuesto = new Repuesto
            {
                codigo_rep = "REP001",
                descripcion = "Filtro aire distinto",
                stock = 3,
                medida_rep = "unidad",
                precio_und = 10.0
            };

            var result = await _controller.Nuevo(repuesto);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un repuesto con este código.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public async Task NuevoRepuesto_DescripcionDuplicada()
        {
            var repuesto = new Repuesto
            {
                codigo_rep = "REP003",
                descripcion = "Filtro de aceite",
                stock = 2,
                medida_rep = "unidad",
                precio_und = 8.0
            };

            var result = await _controller.Nuevo(repuesto);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un repuesto con esta descripción.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public async Task EditarRepuesto_ActualizaDatos()
        {
            var repuesto = new Repuesto
            {
                repuesto_id = 1,
                codigo_rep = "REP001",
                descripcion = "Filtro de aceite actualizado",
                stock = 12,
                medida_rep = "unidad",
                precio_und = 27.5
            };

            var result = await _controller.Editar(repuesto, accionStock: null, valorStock: null);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actualizado = await _context.Repuestos.FindAsync(1);
            Assert.AreEqual("Filtro de aceite actualizado", actualizado.descripcion);
            Assert.AreEqual(12, actualizado.stock);
            Assert.AreEqual(27.5, actualizado.precio_und);
        }

        [TestMethod]
        public async Task EditarRepuesto_CodigoDuplicado()
        {
            // Añadir un segundo repuesto con código distinto para forzar duplicado
            _context.Repuestos.Add(new Repuesto
            {
                repuesto_id = 2,
                codigo_rep = "REPXX",
                descripcion = "Otro repuesto",
                stock = 1,
                medida_rep = "unidad",
                precio_und = 5.0,
                fecha_registro_repuesto = DateTime.Now
            });
            _context.SaveChanges();

            var repuestoEdicion = new Repuesto
            {
                repuesto_id = 1,
                codigo_rep = "REPXX", // pasa a usar el código del segundo repuesto
                descripcion = "Filtro modificado",
                stock = 10,
                medida_rep = "unidad",
                precio_und = 20.0
            };

            var result = await _controller.Editar(repuestoEdicion, accionStock: null, valorStock: null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un repuesto con este código.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public async Task EditarRepuesto_DescripcionDuplicada()
        {
            // Añadir un segundo repuesto con descripción distinta para forzar duplicado
            _context.Repuestos.Add(new Repuesto
            {
                repuesto_id = 3,
                codigo_rep = "REP003",
                descripcion = "Descripción Única",
                stock = 2,
                medida_rep = "unidad",
                precio_und = 7.0,
                fecha_registro_repuesto = DateTime.Now
            });
            _context.SaveChanges();

            var repuestoEdicion = new Repuesto
            {
                repuesto_id = 1,
                codigo_rep = "REP001",
                descripcion = "Descripción Única", // duplicada
                stock = 10,
                medida_rep = "unidad",
                precio_und = 20.0
            };

            var result = await _controller.Editar(repuestoEdicion, accionStock: null, valorStock: null);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("Ya existe un repuesto con esta descripción.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public async Task EditarRepuesto_AumentarStock()
        {
            var repuestoEdicion = new Repuesto
            {
                repuesto_id = 1,
                codigo_rep = "REP001",
                descripcion = "Filtro de aceite",
                stock = 10,
                medida_rep = "unidad",
                precio_und = 25.0
            };

            var result = await _controller.Editar(repuestoEdicion, accionStock: "Aumentar", valorStock: 5);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actualizado = await _context.Repuestos.FindAsync(1);
            Assert.AreEqual(15, actualizado.stock);
        }

        [TestMethod]
        public async Task EditarRepuesto_DisminuirStock()
        {
            var repuestoEdicion = new Repuesto
            {
                repuesto_id = 1,
                codigo_rep = "REP001",
                descripcion = "Filtro de aceite",
                stock = 10,
                medida_rep = "unidad",
                precio_und = 25.0
            };

            var result = await _controller.Editar(repuestoEdicion, accionStock: "Disminuir", valorStock: 3);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var actualizado = await _context.Repuestos.FindAsync(1);
            Assert.AreEqual(7, actualizado.stock);
        }

        [TestMethod]
        public async Task EditarRepuesto_DisminuirStock_MenorCero()
        {
            var repuestoEdicion = new Repuesto
            {
                repuesto_id = 1,
                codigo_rep = "REP001",
                descripcion = "Filtro de aceite",
                stock = 10,
                medida_rep = "unidad",
                precio_und = 25.0
            };

            var result = await _controller.Editar(repuestoEdicion, accionStock: "Disminuir", valorStock: 20);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.AreEqual("No se puede disminuir el stock por debajo de 0.", view.ViewData["mensaje"]);
        }

        [TestMethod]
        public void EliminarRepuesto_MuestraVista()
        {
            var result = _controller.Eliminar(1);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            Assert.IsNotNull(view.Model);
        }

        [TestMethod]
        public void ConfirmacionEliminar_EliminaRepuesto()
        {
            var result = _controller.ConfirmacionEliminar(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var repuesto = _context.Repuestos.Find(1);
            Assert.IsNull(repuesto);
        }

        [TestMethod]
        public async Task Mostrar_BusquedaFiltra()
        {
            // Añadir repuestos adicionales
            _context.Repuestos.Add(new Repuesto
            {
                repuesto_id = 4,
                codigo_rep = "FILTRO-A",
                descripcion = "Filtro de aire A",
                stock = 4,
                medida_rep = "unidad",
                precio_und = 12.0,
                fecha_registro_repuesto = DateTime.Now
            });
            _context.Repuestos.Add(new Repuesto
            {
                repuesto_id = 5,
                codigo_rep = "FILTRO-B",
                descripcion = "Filtro de aceite B",
                stock = 6,
                medida_rep = "unidad",
                precio_und = 18.0,
                fecha_registro_repuesto = DateTime.Now
            });
            _context.SaveChanges();

            var result = await _controller.Mostrar("aceite");

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var view = result as ViewResult;
            var modelo = view.Model as List<Repuesto>;
            Assert.IsTrue(modelo.All(r => r.descripcion.Contains("aceite", StringComparison.OrdinalIgnoreCase) || r.codigo_rep.Contains("aceite", StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(modelo.Count > 0);
        }
    }
}