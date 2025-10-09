using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;

namespace SistemaDeCotizacion.Controllers
{
    public class ServicioController : Controller
    {
        private readonly AppDBContext _appDBContext;

        public ServicioController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar()
        {
            var servicios = _appDBContext.Servicios.ToList();
            return View(servicios);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Servicio servicio)
        {

            if (await _appDBContext.Servicios.AnyAsync(v => v.nombre_servicio == servicio.nombre_servicio))
            {
                ViewBag.Servicios = _appDBContext.Servicios.ToList();
                ViewData["mensaje"] = "Ya existe un servicio con este nombre.";
                return View(servicio);
            }

            servicio.fecha_registro_servicio = DateTime.Now;

            await _appDBContext.Servicios.AddAsync(servicio);
            await _appDBContext.SaveChangesAsync();
            TempData["mensaje"] = "Servicio creado exitosamente.";

            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var servicio = await _appDBContext.Servicios.FindAsync(id);
            if (servicio == null)
                return NotFound();

            return View(servicio);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Servicio servicio)
        {

            var ser = await _appDBContext.Servicios.FindAsync(servicio.servicio_id);
            if (ser == null)
                return NotFound();

            bool nombreRepetido = await _appDBContext.Servicios
                .AnyAsync(s => s.nombre_servicio == servicio.nombre_servicio && s.servicio_id != servicio.servicio_id);
            if (nombreRepetido)
            {
                ViewBag.Servicios = _appDBContext.Servicios.ToList();
                ViewData["mensaje"] = "Ya existe un servicio con este nombre.";
                return View(servicio);
            }

            ser.nombre_servicio = servicio.nombre_servicio;
            ser.precio = servicio.precio;
            ser.detalle_servicio = servicio.detalle_servicio;

            await _appDBContext.SaveChangesAsync();
            TempData["mensaje"] = "Servicio actualizado exitosamente.";
            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var servicio = _appDBContext.Servicios
                .FirstOrDefault(s => s.servicio_id == id);

            if (servicio == null)
                return NotFound();

            return View(servicio);
        }

        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var servicio = _appDBContext.Servicios.Find(id);
            if (servicio == null)
            {
                return NotFound();
            }

            _appDBContext.Servicios.Remove(servicio);
            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Servicio eliminado con éxito.";
            return RedirectToAction(nameof(Mostrar));
        }
    }
}
