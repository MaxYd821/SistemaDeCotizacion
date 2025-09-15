using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;

namespace SistemaDeCotizacion.Controllers
{
    public class CuentaController : Controller
    {
        private readonly AppDBContext _appDBContext;

        public CuentaController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            // Buscar usuario en la base de datos
            var usuario = await _appDBContext.Usuarios
                .Include(u => u.rol)
                .FirstOrDefaultAsync(u => u.correo == loginVM.correo && u.password == loginVM.password);

            // Validar credenciales
            if (usuario == null)
            {
                ViewData["mensaje"] = "Credenciales incorrectas, intente nuevamente.";
                return View();
            }

            // Crear lista de claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.usuario_id.ToString()),
                new Claim(ClaimTypes.Name, usuario.nombre),
                new Claim(ClaimTypes.Email, usuario.correo),
                new Claim(ClaimTypes.Role, usuario.rol.rol_nombre)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Iniciar sesión
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirigir al inicio
            return RedirectToAction("Index", "Home");
        }
    }
}
