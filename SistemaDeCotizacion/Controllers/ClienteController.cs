using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;
using System.Security.Claims;

namespace SistemaDeCotizacion.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public ClienteController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar(string busqueda = null, int? mes = null, int? anio = null, int pagina = 1, int registrosPorPagina = 5)
        {
            var query = _appDBContext.Clientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(c =>
                    c.nombre_cliente.Contains(busqueda) ||
                    c.ruc.Contains(busqueda)
                );
            }

            if (mes.HasValue)
            {
                query = query.Where(c => c.fecha_registro_cliente.Month == mes.Value);
            }

            if (anio.HasValue)
            {
                query = query.Where(c => c.fecha_registro_cliente.Year == anio.Value);
            }

            var totalRegistros = await query.CountAsync();

            var clientes = await query
                .OrderByDescending(c => c.fecha_registro_cliente)
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
            return View(clientes);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Nuevo(Cliente cliente)
        {
            if (await _appDBContext.Clientes.AnyAsync(c => c.correo_cliente == cliente.correo_cliente))
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Correo ya registrado.";
                return View(cliente);
            }

            if (await _appDBContext.Clientes.AnyAsync(c => c.ruc == cliente.ruc))
            {
                ViewBag.Clientes = _appDBContext.Clientes.ToList();
                ViewData["mensaje"] = "RUC/DNI ya registrado.";
                return View(cliente);
            }

            if (cliente.tipo.Equals("Persona Natural"))
            {
                if (cliente.ruc.Length != 8)
                {
                    ViewBag.Clientes = _appDBContext.Clientes.ToList();
                    ViewData["mensaje"] = "La longitud del DNI debe ser de 8 dígitos.";
                    return View(cliente);
                }
            }
            else
            {
                if (cliente.ruc.Length != 11)
                {
                    ViewBag.Clientes = _appDBContext.Clientes.ToList();
                    ViewData["mensaje"] = "La longitud del RUC debe ser de 11 dígitos.";
                    return View(cliente);
                }
            }
            var cl = new Cliente
            {
                nombre_cliente = cliente.nombre_cliente,
                telefono_cliente = cliente.telefono_cliente,
                ruc = cliente.ruc,
                fecha_registro_cliente = DateTime.Now,
                tipo = cliente.tipo,
                correo_cliente = cliente.correo_cliente,
                direccion_cliente = cliente.direccion_cliente,
            };

            await _appDBContext.Clientes.AddAsync(cl);
            await _appDBContext.SaveChangesAsync();

            TempData["mensaje"] = "Cliente registrado con éxito.";

            return RedirectToAction(nameof(Mostrar));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _appDBContext.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            var cliente_edi = new Cliente
            {
                cliente_id = cliente.cliente_id,
                nombre_cliente = cliente.nombre_cliente,
                telefono_cliente = cliente.telefono_cliente,
                ruc = cliente.ruc,
                tipo = cliente.tipo,
                correo_cliente = cliente.correo_cliente,
                direccion_cliente = cliente.direccion_cliente,
            };

            ViewBag.Clientes = _appDBContext.Clientes.ToList();
            return View(cliente_edi);
        }
        [HttpPost]
        public async Task<IActionResult> Editar(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clientes = _appDBContext.Clientes.ToList();
                ViewData["mensaje"] = "Revisa los datos ingresados.";
                return View(cliente);
            }

            var cl = await _appDBContext.Clientes.FindAsync(cliente.cliente_id);
            if (cl == null)
                return NotFound();

            bool correoRepetido = await _appDBContext.Clientes
                .AnyAsync(c => c.correo_cliente == cliente.correo_cliente && c.cliente_id != cliente.cliente_id);
            if (correoRepetido)
            {
                ViewBag.Clientes = _appDBContext.Clientes.ToList();
                ViewData["mensaje"] = "Ya existe un cliente con ese correo.";
                return View(cliente);
            }

            bool dniRepetido = await _appDBContext.Clientes
                .AnyAsync(c => c.ruc == cliente.ruc && c.cliente_id != cliente.cliente_id);
            if (dniRepetido)
            {
                ViewBag.Clientes = _appDBContext.Clientes.ToList();
                ViewData["mensaje"] = "Ya existe un cliente con ese RUC/DNI.";
                return View(cliente);
            }

            if(cliente.tipo.Equals("Persona Natural"))
            {
                if (cliente.ruc.Length != 8)
                {
                    ViewBag.Clientes = _appDBContext.Clientes.ToList();
                    ViewData["mensaje"] = "La longitud del DNI debe ser de 8 dígitos.";
                    return View(cliente);
                }
            }
            else
            {
                if (cliente.ruc.Length != 11)
                {
                    ViewBag.Clientes = _appDBContext.Clientes.ToList();
                    ViewData["mensaje"] = "La longitud del RUC debe ser de 11 dígitos.";
                    return View(cliente);
                }
            }

            cl.nombre_cliente = cliente.nombre_cliente;
            cl.telefono_cliente = cliente.telefono_cliente;
            cl.ruc = cliente.ruc;
            cl.tipo = cliente.tipo;
            cl.correo_cliente = cliente.correo_cliente;
            cl.direccion_cliente = cliente.direccion_cliente;

            await _appDBContext.SaveChangesAsync();

            TempData["mensaje"] = "Cliente actualizado con éxito.";

            return RedirectToAction(nameof(Mostrar));
        }
        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var idCliente = _appDBContext.Clientes.Find(id);
            return View(idCliente);
        }
        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var cliente = _appDBContext.Clientes
                .Include(c => c.vehiculos)
                .Include(c => c.cotizaciones)
                .FirstOrDefault(c => c.cliente_id == id);

            if (cliente == null)
            {
                return NotFound();
            }

            if (cliente.vehiculos != null && cliente.vehiculos.Any())
            {
                TempData["error"] = $"No se puede eliminar el cliente '{cliente.nombre_cliente}' porque hay vehículos asignados. " +
                                    "Elimina primero a todos los vehículos que pertenecen a este cliente.";
                return RedirectToAction(nameof(Mostrar));
            }

            if (cliente.cotizaciones != null && cliente.cotizaciones.Any())
            {
                TempData["error"] = $"No se puede eliminar el cliente '{cliente.nombre_cliente}' porque hay cotizaciones asignadas. " +
                                    "Elimina primero a todas las cotizaciones que pertenecen a este cliente.";
                return RedirectToAction(nameof(Mostrar));
            }
            _appDBContext.Clientes.Remove(cliente);
            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Cliente eliminado con éxito.";
            return RedirectToAction(nameof(Mostrar));
        }
    }
}
