using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;

namespace SistemaDeCotizacion.Controllers
{
    [Authorize]
    public class ServicioController : Controller
    {
        private readonly AppDBContext _appDBContext;

        public ServicioController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar(string busqueda = null, int? mes = null, int? anio = null)
        {
            var query = _appDBContext.Servicios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(s => s.nombre_servicio.Contains(busqueda));
            }

            if (mes.HasValue)
            {
                query = query.Where(s => s.fecha_registro_servicio.Month == mes.Value);
            }

            if (anio.HasValue)
            {
                query = query.Where(s => s.fecha_registro_servicio.Year == anio.Value);
            }

            var servicios = await query
                .OrderByDescending(s => s.fecha_registro_servicio)
                .ToListAsync();

            ViewBag.MesSeleccionado = mes;
            ViewBag.AnioSeleccionado = anio;

            ViewBag.Meses = Enumerable.Range(1, 12)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = System.Globalization.CultureInfo
                    .GetCultureInfo("es-ES")
                    .DateTimeFormat
                    .GetMonthName(i)
                    .ToUpper(),
                    Selected = (i == mes)
                })
                .ToList();
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
            var servicio = _appDBContext.Servicios
                .Include(s => s.detalle_servicio)
                .FirstOrDefault(s => s.servicio_id == id);

            if (servicio == null)
            {
                return NotFound();
            }

            if (servicio.detalle_servicio != null && servicio.detalle_servicio.Any())
            {
                TempData["error"] = $"No se puede eliminar el servicio '{servicio.nombre_servicio}' porque hay cotizaciones asignadas. " +
                                    "Elimina primero a todas las cotizaciones que pertenecen a este servicio.";
                return RedirectToAction(nameof(Mostrar));
            }

            _appDBContext.Servicios.Remove(servicio);
            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Servicio eliminado con éxito.";
            return RedirectToAction(nameof(Mostrar));
        }
    }
}
