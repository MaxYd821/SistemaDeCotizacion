using Microsoft.AspNetCore.Mvc;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using Microsoft.EntityFrameworkCore;

namespace SistemaDeCotizacion.Controllers
{
    public class RolController : Controller
    {
        private readonly AppDBContext _appDBContext;

        public RolController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar()
        {
            var roles = _appDBContext.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Rol rol)
        {

            if (await _appDBContext.Roles.AnyAsync(r => r.rol_nombre == rol.rol_nombre))
            {
                ViewBag.Roles = _appDBContext.Servicios.ToList();
                ViewData["mensaje"] = "Ya existe un rol con este nombre.";
                return View(rol);
            }

            await _appDBContext.Roles.AddAsync(rol);
            await _appDBContext.SaveChangesAsync();

            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var rol = await _appDBContext.Roles.FindAsync(id);
            if (rol == null)
                return NotFound();

            return View(rol);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Rol rol)
        {

            var ro = await _appDBContext.Roles.FindAsync(rol.rol_id);
            if (ro == null)
                return NotFound();

            bool nombreRepetido = await _appDBContext.Roles
                .AnyAsync(r => r.rol_nombre == rol.rol_nombre && r.rol_id != rol.rol_id);
            if (nombreRepetido)
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Ya existe un rol con este nombre.";
                return View(rol);
            }

            ro.rol_nombre = rol.rol_nombre;
            ro.rol_descripcion = rol.rol_descripcion;

            await _appDBContext.SaveChangesAsync();
            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var rol = _appDBContext.Roles
                .FirstOrDefault(r => r.rol_id == id);

            if (rol == null)
                return NotFound();

            return View(rol);
        }

        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var rol = _appDBContext.Roles.Find(id);
            if (rol == null)
            {
                return NotFound();
            }

            _appDBContext.Roles.Remove(rol);
            _appDBContext.SaveChanges();

            return RedirectToAction(nameof(Mostrar));
        }
    }
}
