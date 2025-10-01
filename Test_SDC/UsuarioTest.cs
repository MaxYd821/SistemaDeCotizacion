using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Controllers;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_SDC
{
    [TestClass]
    public class UsuarioTest
    {
        private UsuarioController _usuarioController;
        private AppDBContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDBContext(options);

            _context.Usuarios.Add(new Usuario
            {
                usuario_id=1,
                nombre="Juan",
                apellido="Perez",
                num_cel="999888777",
                dni="12345678",
                fecha_registro=DateTime.Now,
                fecha_nacimiento = new DateTime(1990, 1, 1),
                estado="activo",
                correo="test@correo.com",
                password="12345",
                rol_id=1
            });
            _context.SaveChanges();

            _usuarioController = new UsuarioController(_context);
        }

        [TestMethod]
        public async Task TestUsuarioCreadoOk()
        {
            // Arrange
            RegistroVM registroVM = new RegistroVM
            {
                nombre = "Ana",
                apellido = "Lopez",
                correo = "jg18@gmail.com",
                password = "12345",
                confirmarpassword = "12345",
                dni = "87654321",
                num_cel = "123456789",
                estado = "Activo",
                fecha_nacimiento = new DateTime(1990, 1, 1),
                rol_id = 1
            };

            // Act
            var result = await _usuarioController.Nuevo(registroVM);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);


        }

        [TestMethod]
        public async Task TestUsuarioCorreoDuplicado()
        {
            // Caso 1: Correo duplicado
            var registroCorreoDuplicado = new RegistroVM
            {
                nombre = "Pedro",
                apellido = "Gomez",
                correo = "test@correo.com", // ya existe en Setup
                password = "12345",
                confirmarpassword = "12345",
                dni = "99999999",
                num_cel = "123456789",
                estado = "Activo",
                fecha_nacimiento = new DateTime(1990, 1, 1),
                rol_id = 1
            };
            var resultCorreo = await _usuarioController.Nuevo(registroCorreoDuplicado);
            Assert.IsInstanceOfType(resultCorreo, typeof(ViewResult));
            Assert.AreEqual("Correo ya registrado.", _usuarioController.ViewData["mensaje"]);

            
        }

        [TestMethod]
        public async Task TestUsuarioDniDuplicado()
        {
            // Caso 2: DNI duplicado
            var registroDNIDuplicado = new RegistroVM
            {
                nombre = "Luis",
                apellido = "Martinez",
                correo = "luis@correo.com",
                password = "12345",
                confirmarpassword = "12345",
                dni = "12345678", // ya existe en Setup
                num_cel = "987654321",
                estado = "Activo",
                fecha_nacimiento = new DateTime(1990, 1, 1),
                rol_id = 1
            };
            var resultDNI = await _usuarioController.Nuevo(registroDNIDuplicado);
            Assert.IsInstanceOfType(resultDNI, typeof(ViewResult));
            Assert.AreEqual("DNI ya registrado.", _usuarioController.ViewData["mensaje"]);

            
        }

        [TestMethod]
        public async Task TestUsuarioMenorDeEdad()
        {
            // Caso 3: Edad menor a 18 años
            var registroMenorEdad = new RegistroVM
            {
                nombre = "Sofia",
                apellido = "Torres",
                correo = "sofia@correo.com",
                password = "12345",
                confirmarpassword = "12345",
                dni = "88888888",
                num_cel = "123456789",
                estado = "Activo",
                fecha_nacimiento = DateTime.Today.AddYears(-17), // menor de 18
                rol_id = 1
            };
            var resultEdad = await _usuarioController.Nuevo(registroMenorEdad);
            Assert.IsInstanceOfType(resultEdad, typeof(ViewResult));
            Assert.AreEqual("Debes tener al menos 18 años.", _usuarioController.ViewData["mensaje"]);
        }
        [TestMethod]
        public async Task TestUsuarioEditadoOk()
        {
            // Arrange
            Usuario u = new Usuario
            {
                nombre = "Luis",
                apellido = "Gomez",
                correo = "luis@correo.com",
                password = "12345",
                dni = "87654321",
                num_cel = "123456789",
                estado = "Activo",
                fecha_nacimiento = new DateTime(1990, 1, 1),
                rol_id = 1
            };

            _context.Usuarios.Add(u);
            _context.SaveChanges();

            EditarUsuarioVM editarVM = new EditarUsuarioVM
            {
                usuario_id=u.usuario_id,
                nombre = u.nombre,
                apellido = u.apellido,
                correo = u.correo,
                password = "54321",
                dni = u.dni,
                num_cel = u.num_cel,
                estado = u.estado,
                fecha_nacimiento = u.fecha_nacimiento,
                rol_id = u.rol_id
            };

            // Act
            var result = await _usuarioController.Editar(editarVM);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);


        }
        [TestMethod]
        public async Task TestUsuarioMostrar()
        {
            // Arrange


            // Action
            var result = await _usuarioController.Mostrar();


            //Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(List<Usuario>));
        }

        [TestMethod]
        public void TestEliminarUsuario()
        {
            var usuario = new Usuario
            {
                nombre = "Carlos",
                apellido = "Diaz",
                correo = "carlos@correo.com",
                password = "12345",
                dni = "11223344",
                num_cel = "777777777",
                estado = "Activo",
                fecha_nacimiento = new DateTime(1985, 3, 3),
                rol_id = 1
            };
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            var result = _usuarioController.ConfirmacionEliminar(usuario.usuario_id);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Mostrar", redirect.ActionName);
        }

    }
}
