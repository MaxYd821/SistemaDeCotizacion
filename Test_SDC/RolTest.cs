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
using System.Threading.Tasks;

namespace Test_SDC
{
    [TestClass]
    public class RolTest
    {
        private RolController _controller;
        private AppDBContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDBContext(options);

            _context.Roles.Add(new Rol
            {
                rol_id = 1,
                rol_nombre = "Administrador",
                rol_descripcion = "Acceso total"
            });
            _context.SaveChanges();

            _controller = new RolController(_context)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
        }

        // ✅ 1. Mostrar todos los roles
        [TestMethod]
        public async Task Mostrar_DeberiaRetornarListaRoles()
        {
            var result = await _controller.Mostrar() as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as List<Rol>;
            Assert.IsTrue(model.Any());
        }

        // ✅ 2. Mostrar filtrando por nombre
        [TestMethod]
        public async Task Mostrar_DeberiaFiltrarPorNombre()
        {
            var result = await _controller.Mostrar("Admin") as ViewResult;

            Assert.IsNotNull(result);
            var model = result.Model as List<Rol>;
            Assert.AreEqual(1, model.Count);
            Assert.AreEqual("Administrador", model.First().rol_nombre);
        }

        // ✅ 3. Crear un rol nuevo correctamente
        [TestMethod]
        public async Task Nuevo_CrearRolExitosamente()
        {
            var nuevoRol = new Rol
            {
                rol_nombre = "Supervisor",
                rol_descripcion = "Supervisa tareas"
            };

            var result = await _controller.Nuevo(nuevoRol);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);
        }

        // ✅ 4. Intentar crear un rol duplicado
        [TestMethod]
        public async Task Nuevo_RolDuplicado_DeberiaMostrarMensaje()
        {
            var rolDuplicado = new Rol
            {
                rol_nombre = "Administrador",
                rol_descripcion = "Duplicado"
            };

            var result = await _controller.Nuevo(rolDuplicado);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.AreEqual("Ya existe un rol con este nombre.", _controller.ViewData["mensaje"]);
        }

        // ✅ 5. Editar un rol correctamente
        [TestMethod]
        public async Task Editar_RolActualizadoCorrectamente()
        {
            var rolEditado = new Rol
            {
                rol_id = 1,
                rol_nombre = "Administrador General",
                rol_descripcion = "Acceso completo"
            };

            var result = await _controller.Editar(rolEditado);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var rolActualizado = _context.Roles.Find(1);
            Assert.AreEqual("Administrador General", rolActualizado.rol_nombre);
        }

        // ✅ 6. Editar un rol con nombre repetido
        [TestMethod]
        public async Task Editar_RolConNombreDuplicado_DeberiaRetornarView()
        {
            _context.Roles.Add(new Rol
            {
                rol_id = 2,
                rol_nombre = "Supervisor",
                rol_descripcion = "Supervisa tareas"
            });
            _context.SaveChanges();

            var rolDuplicado = new Rol
            {
                rol_id = 2,
                rol_nombre = "Administrador", // Ya existe
                rol_descripcion = "Prueba duplicado"
            };

            var result = await _controller.Editar(rolDuplicado);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual("Ya existe un rol con este nombre.", _controller.ViewData["mensaje"]);
        }

        // ✅ 7. Editar un rol que no existe
        [TestMethod]
        public async Task Editar_RolNoExiste_DeberiaRetornarNotFound()
        {
            var rolInexistente = new Rol
            {
                rol_id = 99,
                rol_nombre = "Desconocido",
                rol_descripcion = "No existe"
            };

            var result = await _controller.Editar(rolInexistente);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // ✅ 8. Eliminar un rol correctamente
        [TestMethod]
        public void ConfirmacionEliminar_RolEliminadoCorrectamente()
        {
            var result = _controller.ConfirmacionEliminar(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);
            Assert.IsFalse(_context.Roles.Any(r => r.rol_id == 1));
        }

        // ✅ 9. Intentar eliminar un rol con usuarios asignados
        [TestMethod]
        public void ConfirmacionEliminar_RolConUsuarios_NoDebeEliminarse()
        {
            var rol = new Rol
            {
                rol_id = 3,
                rol_nombre = "Recepcionista",
                rol_descripcion = "Gestiona atención al cliente",
                usuarios = new List<Usuario> { new Usuario { usuario_id = 1, nombre = "Test", 
                    apellido = "Ramírez",
                    correo = "carlos@test.com",
                    dni = "12345678",
                    estado = "Activo",
                    num_cel = "987654321",
                    password = "123456",
                    rol_id = 3} }
            };
            _context.Roles.Add(rol);
            _context.SaveChanges();

            var result = _controller.ConfirmacionEliminar(3) as RedirectToActionResult;

            Assert.AreEqual("Mostrar", result.ActionName);
            Assert.IsNotNull(_controller.TempData["error"]);
            Assert.IsTrue(_context.Roles.Any(r => r.rol_id == 3));
        }

        // ✅ 10. Intentar eliminar un rol que no existe
        [TestMethod]
        public void ConfirmacionEliminar_RolNoExiste_DeberiaRetornarNotFound()
        {
            var result = _controller.ConfirmacionEliminar(999);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }
    }
}
