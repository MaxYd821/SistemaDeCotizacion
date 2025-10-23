using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;
using System.Globalization;
using System.Security.Claims;

namespace SistemaDeCotizacion.Controllers
{
    public class IngresoController : Controller
    {
        private readonly AppDBContext _appDBContext;

        public IngresoController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar(string busqueda = null, int? mes = null, int? anio = null)
        {
            var query = _appDBContext.Ingresos
                .Include(i => i.usuario)
                    .ThenInclude(u => u.clientes)
                        .ThenInclude(c => c.cotizaciones)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(i =>
                    i.usuario.nombre.Contains(busqueda) ||
                    i.usuario.apellido.Contains(busqueda) ||
                    i.tipo_ingreso.Contains(busqueda)
                );
            }

            if (mes.HasValue)
            {
                query = query.Where(i => i.fecha_ingreso.Month == mes.Value);
            }
            if (anio.HasValue)
            {
                query = query.Where(i => i.fecha_ingreso.Year == anio.Value);
            }

            var ingresos = await query
                .OrderByDescending(i => i.fecha_ingreso)
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

            return View(ingresos);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            var cotizacionesAprobadas = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                .Where(c => c.estado_cotizacion == "Aprobado")
                .ToList();

            ViewBag.Cotizaciones = cotizacionesAprobadas;
            ViewBag.Usuarios = _appDBContext.Usuarios.ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Nuevo(Ingreso ingreso, int cotizacion_id)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (usuarioIdClaim == null)
            {
                ViewData["mensaje"] = "No se pudo identificar al usuario logueado.";
                return View(ingreso);
            }

            ingreso.usuario_id = int.Parse(usuarioIdClaim.Value);

            var cotizacion = _appDBContext.Cotizaciones
                    .FirstOrDefault(c => c.cotizacion_id == cotizacion_id);

                if (cotizacion != null)
                {
                    
                    ingreso.costo_ingreso = cotizacion.costo_servicio_total + cotizacion.costo_repuesto_total;
                    ingreso.fecha_registro_ingreso = DateTime.Now;
                    ingreso.detalle_ingreso = $"Ingreso generado a partir de la cotización ID: #{cotizacion.cotizacion_id}";

                    _appDBContext.Ingresos.Add(ingreso);
                    cotizacion.estado_cotizacion = "Ingresado";
                    _appDBContext.Cotizaciones.Update(cotizacion);
                    _appDBContext.SaveChanges();

                    TempData["mensaje"] = "Ingreso registrado exitosamente.";
                    return RedirectToAction("Mostrar");
                }

                ModelState.AddModelError("", "No se encontró la cotización seleccionada.");
            

            ViewBag.Cotizaciones = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                .Where(c => c.estado_cotizacion == "Aprobado")
                .ToList();

            ViewBag.Usuarios = _appDBContext.Usuarios.ToList();

            return View(ingreso);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var ingreso = _appDBContext.Ingresos
                .Include(i => i.usuario)
                .FirstOrDefault(i => i.ingreso_id == id);

            if (ingreso == null)
            {
                return NotFound();
            }

            int? cotizacionIdAsociada = null;
            var match = System.Text.RegularExpressions.Regex.Match(ingreso.detalle_ingreso ?? "", @"#(\d+)");
            if (match.Success)
            {
                cotizacionIdAsociada = int.Parse(match.Groups[1].Value);
            }

            var cotizacionesAprobadas = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                .Where(c => c.estado_cotizacion == "Aprobado")
                .ToList();

            ViewBag.Cotizaciones = cotizacionesAprobadas;
            ViewBag.CotizacionSeleccionada = cotizacionIdAsociada;

            return View(ingreso);
        }

        [HttpPost]
        public IActionResult Editar(Ingreso ingreso, int cotizacion_id)
        {
            var ingresoExistente = _appDBContext.Ingresos.FirstOrDefault(i => i.ingreso_id == ingreso.ingreso_id);

            if (ingresoExistente == null)
            {
                TempData["mensaje"] = "El ingreso no existe o fue eliminado.";
                return RedirectToAction("Mostrar");
            }

            var cotizacion = _appDBContext.Cotizaciones
                .FirstOrDefault(c => c.cotizacion_id == cotizacion_id);

            if (cotizacion == null)
            {
                ViewData["mensaje"] = "No se encontró la cotización seleccionada.";
                ViewBag.Cotizaciones = _appDBContext.Cotizaciones
                    .Include(c => c.cliente)
                    .Where(c => c.estado_cotizacion == "Aprobado")
                    .ToList();
                return View(ingreso);
            }

            // Actualizamos los campos editables
            ingresoExistente.fecha_ingreso = ingreso.fecha_ingreso;
            ingresoExistente.tipo_ingreso = ingreso.tipo_ingreso;
            ingresoExistente.detalle_ingreso = $"Ingreso actualizado a partir de la cotización ID: #{cotizacion.cotizacion_id}";
            ingresoExistente.costo_ingreso = cotizacion.costo_servicio_total + cotizacion.costo_repuesto_total;

            _appDBContext.Ingresos.Update(ingresoExistente);
            cotizacion.estado_cotizacion = "Ingresado";
            _appDBContext.Cotizaciones.Update(cotizacion);
            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Ingreso actualizado correctamente.";
            return RedirectToAction("Mostrar");
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var ingreso = _appDBContext.Ingresos
                .Include(i => i.usuario)
                .FirstOrDefault(i => i.ingreso_id == id);

            if (ingreso == null)
            {
                return NotFound();
            }

            int? cotizacionIdAsociada = null;
            var match = System.Text.RegularExpressions.Regex.Match(ingreso.detalle_ingreso ?? "", @"#(\d+)");
            if (match.Success)
            {
                cotizacionIdAsociada = int.Parse(match.Groups[1].Value);
            }

            var cotizacionesAprobadas = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                .Where(c => c.estado_cotizacion == "Aprobado")
                .ToList();

            ViewBag.Cotizaciones = cotizacionesAprobadas;
            ViewBag.CotizacionSeleccionada = cotizacionIdAsociada;

            return View(ingreso);
        }
        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var ingreso = _appDBContext.Ingresos.Find(id);
            if (ingreso == null)
            {
                return NotFound();
            }

            int? cotizacionIdAsociada = null;
            var match = System.Text.RegularExpressions.Regex.Match(ingreso.detalle_ingreso ?? "", @"#(\d+)");
            if (match.Success)
            {
                cotizacionIdAsociada = int.Parse(match.Groups[1].Value);
            }

            _appDBContext.Ingresos.Remove(ingreso);

            if (cotizacionIdAsociada.HasValue)
            {
                var cotizacion = _appDBContext.Cotizaciones
                    .FirstOrDefault(c => c.cotizacion_id == cotizacionIdAsociada.Value);

                if (cotizacion != null)
                {
                    cotizacion.estado_cotizacion = "Aprobado";
                    _appDBContext.Cotizaciones.Update(cotizacion);
                }
            }

            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Ingreso eliminado con éxito.";
            return RedirectToAction(nameof(Mostrar));
        }
    }
}
