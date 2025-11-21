using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;

namespace SistemaDeCotizacion.Controllers
{
    [Authorize]
    [Authorize(Policy = "Activo")]
    public class RepuestoController : Controller
    {
        private readonly AppDBContext _appDBContext;

        public RepuestoController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar(string busqueda = null, int? mes = null, int? anio = null, int pagina = 1, int registrosPorPagina = 10)
        {
            var query = _appDBContext.Repuestos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(r => 
                    r.codigo_rep.Contains(busqueda) ||
                    r.descripcion.Contains(busqueda)
                );
            }

            if (mes.HasValue)
            {
                query = query.Where(r => r.fecha_registro_repuesto.Month == mes.Value);
            }

            if (anio.HasValue)
            {
                query = query.Where(r => r.fecha_registro_repuesto.Year == anio.Value);
            }

            var totalRegistros = await query.CountAsync();

            var repuestos = await query
                .OrderByDescending(r => r.fecha_registro_repuesto)
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToListAsync();

            ViewBag.MesSeleccionado = mes;
            ViewBag.AnioSeleccionado = anio;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

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
            return View(repuestos);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Repuesto repuesto)
        {
            if (await _appDBContext.Repuestos.AnyAsync(r => r.codigo_rep == repuesto.codigo_rep))
            {
                ViewBag.Repuestos = await _appDBContext.Repuestos.ToListAsync();
                ViewData["mensaje"] = "Ya existe un repuesto con este código.";
                return View(repuesto);
            }

            if (await _appDBContext.Repuestos.AnyAsync(r => r.descripcion == repuesto.descripcion))
            {
                ViewBag.Repuestos = await _appDBContext.Repuestos.ToListAsync();
                ViewData["mensaje"] = "Ya existe un repuesto con esta descripción.";
                return View(repuesto);
            }

            repuesto.fecha_registro_repuesto = DateTime.Now;

            await _appDBContext.Repuestos.AddAsync(repuesto);
            await _appDBContext.SaveChangesAsync();
            TempData["mensaje"] = "Repuesto creado exitosamente.";

            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var repuesto = await _appDBContext.Repuestos.FindAsync(id);
            if (repuesto == null)
                return NotFound();

            return View(repuesto);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Repuesto repuesto, string accionStock, int? valorStock)
        {
            var rep = await _appDBContext.Repuestos.FindAsync(repuesto.repuesto_id);
            if (rep == null)
                return NotFound();

            bool codigoRepetido = await _appDBContext.Repuestos
                .AnyAsync(r => r.codigo_rep == repuesto.codigo_rep && r.repuesto_id != repuesto.repuesto_id);
            if (codigoRepetido)
            {
                ViewBag.Repuestos = await _appDBContext.Repuestos.ToListAsync();
                ViewData["mensaje"] = "Ya existe un repuesto con este código.";
                return View(repuesto);
            }

            bool descripcionRepetida = await _appDBContext.Repuestos
                .AnyAsync(r => r.descripcion == repuesto.descripcion && r.repuesto_id != repuesto.repuesto_id);
            if (descripcionRepetida)
            {
                ViewBag.Repuestos = await _appDBContext.Repuestos.ToListAsync();
                ViewData["mensaje"] = "Ya existe un repuesto con esta descripción.";
                return View(repuesto);
            }

            rep.codigo_rep = repuesto.codigo_rep;
            rep.descripcion = repuesto.descripcion;
            rep.stock = repuesto.stock;
            rep.medida_rep = repuesto.medida_rep;
            rep.precio_und = repuesto.precio_und;

            if (!string.IsNullOrEmpty(accionStock) && valorStock.HasValue)
            {
                if (accionStock == "Aumentar")
                {
                    rep.stock += valorStock.Value;
                }
                else if (accionStock == "Disminuir")
                {
                    if (rep.stock - valorStock.Value < 0)
                    {
                        ViewData["mensaje"] = "No se puede disminuir el stock por debajo de 0.";
                        return View(repuesto);
                    }
                    rep.stock -= valorStock.Value;
                }
            }
            else
            {
                rep.stock = repuesto.stock;
            }

            await _appDBContext.SaveChangesAsync();
            TempData["mensaje"] = "Repuesto actualizado exitosamente.";
            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var repuesto = _appDBContext.Repuestos
                .FirstOrDefault(r => r.repuesto_id == id);

            if (repuesto == null)
                return NotFound();

            return View(repuesto);
        }

        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var repuesto = _appDBContext.Repuestos
                .Include(r => r.detalle_repuesto)
                .FirstOrDefault(r => r.repuesto_id == id);
            
            if (repuesto == null)
            {
                return NotFound();
            }

            if (repuesto.detalle_repuesto != null && repuesto.detalle_repuesto.Any())
            {
                TempData["error"] = $"No se puede eliminar el repuesto '{repuesto.descripcion}' porque hay cotizaciones asignadas. " +
                                    "Elimina primero a todas las cotizaciones que pertenecen a este repuesto.";
                return RedirectToAction(nameof(Mostrar));
            }

            _appDBContext.Repuestos.Remove(repuesto);
            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Repuesto eliminado con éxito.";
            return RedirectToAction(nameof(Mostrar));
        }
    }
}
