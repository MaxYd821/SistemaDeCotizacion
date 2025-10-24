using Azure.Core;
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
    public class CotizacionController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public CotizacionController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar(string busqueda = null, int? mes = null, int? anio = null, string estado = null)
        {
            var query = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                    .ThenInclude(c => c.vehiculos)
                .Include(c => c.servicios)
                .Include(c => c.repuestos)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(c =>
                    c.cliente.nombre_cliente.Contains(busqueda) ||
                    c.cliente.ruc.Contains(busqueda) ||
                    c.cliente.vehiculos.Any(v => v.placa.Contains(busqueda))
                );
            }

            if (mes.HasValue)
            {
                query = query.Where(c => c.fecha_cotizacion.Month == mes.Value);
            }
            if (anio.HasValue)
            {
                query = query.Where(c => c.fecha_cotizacion.Year == anio.Value);
            }
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(c => c.estado_cotizacion == estado);
            }

            var cotizaciones = await query
                .OrderByDescending(c => c.fecha_cotizacion)
                .ToListAsync();

            ViewBag.MesSeleccionado = mes;
            ViewBag.AnioSeleccionado = anio;
            ViewBag.EstadoSeleccionado = estado;

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

            return View(cotizaciones);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            var vm = new CotizacionVM
            {
                Clientes = _appDBContext.Clientes.ToList(),
                Vehiculos = _appDBContext.Vehiculos.ToList(),
                Servicios = _appDBContext.Servicios.ToList(),
                Repuestos = _appDBContext.Repuestos.ToList()
            };
            return View(vm);
        }

        [HttpGet]
        public JsonResult ObtenerVehiculosPorCliente(int clienteId)
        {
            var vehiculos = _appDBContext.Vehiculos
                .Where(v => v.cliente_id == clienteId)
                .Select(v => new
                {
                    v.vehiculo_id,
                    v.placa,
                    DisplayName = v.marca + " " + v.modelo + " (" + v.placa + ")"
                })
                .ToList();

            return Json(vehiculos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Nuevo(CotizacionVM model)
        {
            try
            {
                if (model.ClienteId == null || model.VehiculoId == null)
                {
                    ViewData["mensaje"] = "Debe seleccionar un cliente y un vehículo.";
                    model.Clientes = _appDBContext.Clientes.ToList();
                    model.Vehiculos = _appDBContext.Vehiculos
                                .Where(v => v.cliente_id == model.ClienteId)
                                .ToList();
                    model.Servicios = _appDBContext.Servicios.ToList();
                    model.Repuestos = _appDBContext.Repuestos.ToList();
                    return View(model);
                }

                var trabajador = User.Identity?.Name ?? "Desconocido";

                double totalServicios = 0;
                double totalRepuestos = 0;

                var cotizacion = new Cotizacion
                {
                    cliente_id = model.ClienteId,
                    fecha_cotizacion = DateTime.Now,
                    formaPago = model.formaPago ?? string.Empty,
                    tiempoEntrega = model.tiempoEntrega,
                    estado_cotizacion = model.estado_cotizacion ?? "Pendiente",
                    trabajador = trabajador,
                    servicios = new List<DetalleServicio>(),
                    repuestos = new List<DetalleRepuesto>()
                };

                // Servicios
                foreach (var serSel in model.ServiciosSeleccionados ?? Enumerable.Empty<ServicioSeleccionadoVM>())
                {
                    var servicio = await _appDBContext.Servicios.FindAsync(serSel.ServicioId);
                    if (servicio == null) continue;

                    totalServicios += servicio.precio;

                    cotizacion.servicios.Add(new DetalleServicio
                    {
                        servicio_id = servicio.servicio_id
                    });
                }

                // Repuestos
                foreach (var repSel in model.RepuestosSeleccionados ?? Enumerable.Empty<RepuestoSeleccionadoVM>())
                {
                    var repuesto = await _appDBContext.Repuestos.FindAsync(repSel.RepuestoId);
                    if (repuesto == null) continue;

                    if (repuesto.stock < repSel.Cantidad)
                    {
                        ViewData["mensaje"] = $"El repuesto '{repuesto.descripcion}' no tiene suficiente stock. Disponible: {repuesto.stock}";
                        model.Clientes = _appDBContext.Clientes.ToList();
                        model.Vehiculos = _appDBContext.Vehiculos
                            .Where(v => v.cliente_id == model.ClienteId)
                            .ToList();
                        model.Servicios = _appDBContext.Servicios.ToList();
                        model.Repuestos = _appDBContext.Repuestos.ToList();
                        return View(model);
                    }

                    double valorVenta = repSel.Cantidad * repuesto.precio_und;
                    totalRepuestos += valorVenta;
                    repuesto.stock -= repSel.Cantidad;

                    cotizacion.repuestos.Add(new DetalleRepuesto
                    {
                        repuesto_id = repuesto.repuesto_id,
                        cantidad_rep = repSel.Cantidad,
                        valor_venta = valorVenta
                    });

                    _appDBContext.Repuestos.Update(repuesto);
                }

                cotizacion.costo_servicio_total = totalServicios;
                cotizacion.costo_repuesto_total = totalRepuestos;

                await _appDBContext.Cotizaciones.AddAsync(cotizacion);
                await _appDBContext.SaveChangesAsync();

                TempData["mensaje"] = "Cotización registrada exitosamente.";
                return RedirectToAction("Mostrar");
            }
            catch (Exception ex)
            {
                ViewData["mensaje"] = "Error al registrar la cotización: " + ex.Message;
                model.Clientes = _appDBContext.Clientes.ToList();
                model.Vehiculos = _appDBContext.Vehiculos
                            .Where(v => v.cliente_id == model.ClienteId)
                            .ToList();
                model.Servicios = _appDBContext.Servicios.ToList();
                model.Repuestos = _appDBContext.Repuestos.ToList();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var cotizacion = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                    .ThenInclude(c => c.vehiculos)
                .Include(c => c.servicios)
                    .ThenInclude(ds => ds.servicio)
                .Include(c => c.repuestos)
                    .ThenInclude(dr => dr.repuesto)
                .FirstOrDefault(c => c.cotizacion_id == id);

            if (cotizacion == null)
            {
                return NotFound();
            }

            var vm = new CotizacionVM
            {
                CotizacionId = cotizacion.cotizacion_id,
                ClienteId = cotizacion.cliente_id,
                VehiculoId = cotizacion.cliente.vehiculos.FirstOrDefault()?.vehiculo_id, 
                formaPago = cotizacion.formaPago,
                tiempoEntrega = cotizacion.tiempoEntrega,
                estado_cotizacion = cotizacion.estado_cotizacion,
                trabajador = cotizacion.trabajador,

                ServiciosSeleccionados = cotizacion.servicios.Select(s => new ServicioSeleccionadoVM
                {
                    ServicioId = s.servicio_id,
                    precio = s.servicio.precio
                }).ToList(),

                RepuestosSeleccionados = cotizacion.repuestos.Select(r => new RepuestoSeleccionadoVM
                {
                    RepuestoId = r.repuesto_id,
                    Cantidad = r.cantidad_rep
                }).ToList(),

                Clientes = _appDBContext.Clientes.ToList(),
                Vehiculos = _appDBContext.Vehiculos
                    .Where(v => v.cliente_id == cotizacion.cliente_id)
                    .ToList(),
                Servicios = _appDBContext.Servicios.ToList(),
                Repuestos = _appDBContext.Repuestos.ToList()
            };

            ViewBag.Clientes = new SelectList(_appDBContext.Clientes, "cliente_id", "nombre_cliente", vm.ClienteId);
            ViewBag.Vehiculos = new SelectList(_appDBContext.Vehiculos.Where(v => v.cliente_id == vm.ClienteId), "vehiculo_id", "placa", vm.VehiculoId);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CotizacionVM model)
        {
            try
            {
                var cotizacion = _appDBContext.Cotizaciones
                    .Include(c => c.repuestos).ThenInclude(r => r.repuesto)
                    .Include(c => c.servicios)
                    .FirstOrDefault(c => c.cotizacion_id == id);

                if (cotizacion == null)
                {
                    return NotFound();
                }

                if (model.ClienteId == null || model.VehiculoId == null)
                {
                    ViewData["mensaje"] = "Debe seleccionar un cliente.";
                    model.Clientes = _appDBContext.Clientes.ToList();
                    model.Vehiculos = _appDBContext.Vehiculos
                                .Where(v => v.cliente_id == model.ClienteId)
                                .ToList();
                    model.Servicios = _appDBContext.Servicios.ToList();
                    model.Repuestos = _appDBContext.Repuestos.ToList();
                    return View(model);
                }

                // 🧹 Restaurar stock anterior antes de modificar
                foreach (var detRep in cotizacion.repuestos)
                {
                    var rep = _appDBContext.Repuestos.Find(detRep.repuesto_id);
                    if (rep != null)
                    {
                        rep.stock += detRep.cantidad_rep; // devolvemos al stock
                    }
                }

                // 🧹 Eliminar detalles antiguos
                _appDBContext.DetalleServicios.RemoveRange(cotizacion.servicios);
                _appDBContext.DetalleRepuestos.RemoveRange(cotizacion.repuestos);
                _appDBContext.SaveChanges();

                double totalServicios = 0;
                double totalRepuestos = 0;

                // 🔹 Actualizar datos generales
                cotizacion.cliente_id = model.ClienteId;
                cotizacion.formaPago = model.formaPago;
                cotizacion.tiempoEntrega = model.tiempoEntrega;
                cotizacion.estado_cotizacion = model.estado_cotizacion;
                cotizacion.servicios = new List<DetalleServicio>();
                cotizacion.repuestos = new List<DetalleRepuesto>();

                // 🔹 Servicios seleccionados
                foreach (var serSel in model.ServiciosSeleccionados)
                {
                    var servicio = _appDBContext.Servicios.Find(serSel.ServicioId);
                    if (servicio == null) continue;

                    totalServicios += servicio.precio;

                    cotizacion.servicios.Add(new DetalleServicio
                    {
                        servicio_id = servicio.servicio_id
                    });
                }

                // 🔹 Repuestos seleccionados
                foreach (var repSel in model.RepuestosSeleccionados)
                {
                    var repuesto = _appDBContext.Repuestos.Find(repSel.RepuestoId);
                    if (repuesto == null) continue;

                    if (repuesto.stock < repSel.Cantidad)
                    {
                        ViewData["mensaje"] = $"El repuesto '{repuesto.descripcion}' no tiene suficiente stock. Disponible: {repuesto.stock}";
                        model.Clientes = _appDBContext.Clientes.ToList();
                        model.Vehiculos = _appDBContext.Vehiculos
                            .Where(v => v.cliente_id == model.ClienteId)
                            .ToList();
                        model.Servicios = _appDBContext.Servicios.ToList();
                        model.Repuestos = _appDBContext.Repuestos.ToList();
                        return View(model);
                    }

                    double valorVenta = repSel.Cantidad * repuesto.precio_und;
                    totalRepuestos += valorVenta;
                    repuesto.stock -= repSel.Cantidad;

                    cotizacion.repuestos.Add(new DetalleRepuesto
                    {
                        repuesto_id = repuesto.repuesto_id,
                        cantidad_rep = repSel.Cantidad,
                        valor_venta = valorVenta
                    });
                }

                // 🔹 Actualizar totales
                cotizacion.costo_servicio_total = totalServicios;
                cotizacion.costo_repuesto_total = totalRepuestos;

                _appDBContext.Update(cotizacion);
                await _appDBContext.SaveChangesAsync();

                TempData["mensaje"] = "Cotización actualizada exitosamente.";
                return RedirectToAction("Mostrar");
            }
            catch (Exception ex)
            {
                ViewData["mensaje"] = "Error al editar la cotización: " + ex.Message;
                model.Clientes = _appDBContext.Clientes.ToList();
                model.Vehiculos = _appDBContext.Vehiculos
                                .Where(v => v.cliente_id == model.ClienteId)
                                .ToList();
                model.Servicios = _appDBContext.Servicios.ToList();
                model.Repuestos = _appDBContext.Repuestos.ToList();
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var cotizacion = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                    .ThenInclude(c => c.vehiculos)
                .Include(c => c.servicios)
                    .ThenInclude(ds => ds.servicio)
                .Include(c => c.repuestos)
                    .ThenInclude(dr => dr.repuesto)
                .FirstOrDefault(c => c.cotizacion_id == id);

            if (cotizacion == null)
            {
                return NotFound();
            }

            return View(cotizacion);
        }
        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var cotizacion = _appDBContext.Cotizaciones
                .Include(c => c.cliente)
                    .ThenInclude(c => c.vehiculos)
                .Include(c => c.servicios)
                    .ThenInclude(ds => ds.servicio)
                .Include(c => c.repuestos)
                    .ThenInclude(dr => dr.repuesto)
                .FirstOrDefault(c => c.cotizacion_id == id);

            if (cotizacion == null)
            {
                return NotFound();
            }

            var ingresoAsociado = _appDBContext.Ingresos
                .FirstOrDefault(i => i.detalle_ingreso.Contains($"#{cotizacion.cotizacion_id}"));

            if (ingresoAsociado != null)
            {
                TempData["error"] = $"No se puede eliminar la cotización #{cotizacion.cotizacion_id} porque está asociada " +
                                    $"al ingreso ID: {ingresoAsociado.ingreso_id}. " +
                                    $"Por favor, elimine primero dicho ingreso.";
                return RedirectToAction(nameof(Mostrar));
            }

            foreach (var dr in cotizacion.repuestos)
            {
                var repuesto = dr.repuesto;
                if (repuesto != null)
                {
                    repuesto.stock += dr.cantidad_rep; // devuelve el stock al inventario
                    _appDBContext.Repuestos.Update(repuesto);
                }
            }

            _appDBContext.DetalleServicios.RemoveRange(cotizacion.servicios);
            _appDBContext.DetalleRepuestos.RemoveRange(cotizacion.repuestos);

            _appDBContext.Cotizaciones.Remove(cotizacion);
            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Cotización eliminada con éxito.";
            return RedirectToAction(nameof(Mostrar));
        }
    }
}
