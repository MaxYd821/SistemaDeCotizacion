using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SistemaDeCotizacion.Controllers;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Test_SDC
{
    [TestClass]
    public class LoginTest
    {
        private CuentaController _controller;
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
                usuario_id = 1,
                nombre = "Max",
                apellido = "Ramirez",
                correo = "test@correo.com",
                password = "12345",
                dni = "12345678",
                num_cel = "987654321",
                estado = "Activo",
                rol = new Rol
                {
                    rol_id = 1,
                    rol_nombre = "Admin",
                    rol_descripcion = "Administrador del sistema"
                }
            });
            _context.SaveChanges();

            _controller = new CuentaController(_context);

            var httpContext = new DefaultHttpContext();

            // Mock de IAuthenticationService
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock
                .Setup(x => x.SignInAsync(
                    It.IsAny<HttpContext>(),
                    It.IsAny<string>(),
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<AuthenticationProperties>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddSingleton<IAuthenticationService>(authServiceMock.Object);
            httpContext.RequestServices = services.BuildServiceProvider();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // 👉 Inyectar un UrlHelper falso
            _controller.Url = new UrlHelper(new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            });

            // 👉 Simulación de TempData
            var tempDataProvider = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>();
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                httpContext,
                tempDataProvider.Object
            );

        }


        [TestMethod]
        public async Task TestLoginOK()
        {
            // Arrange
            var loginVM = new LoginVM { correo = "test@correo.com", password = "12345" };

            // Act
            var result = await _controller.Login(loginVM);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirect = result as RedirectToActionResult;
            Assert.AreEqual("Index", redirect.ActionName);
            Assert.AreEqual("Home", redirect.ControllerName);
        }

        [TestMethod]
        public async Task TestLoginFail()
        {
            // Arrange
            var loginVM = new LoginVM { correo = "fake@correo.com", password = "wrong" };

            // Act
            var result = await _controller.Login(loginVM);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.IsTrue(_controller.ViewData.ContainsKey("mensaje"));
            Assert.AreEqual("Credenciales incorrectas, intente nuevamente.", _controller.ViewData["mensaje"]);
        }

    }
}