using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaDeCotizacion.Controllers
{
    public class VehiculoController : Controller
    {
        private readonly AppDBContext _appDBContext;

        public VehiculoController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar()
        {
            var vehiculos = await _appDBContext.Vehiculos
                .Include(v => v.cliente)
                .ToListAsync();
            return View(vehiculos);
        }

        [HttpGet]
        public IActionResult Nuevo()
        {
            ViewBag.Clientes = _appDBContext.Clientes.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(Vehiculo vehiculo)
        {

            if (await _appDBContext.Vehiculos.AnyAsync(v => v.placa == vehiculo.placa))
            {
                ViewBag.Clientes = _appDBContext.Clientes.ToList();
                ViewData["mensaje"] = "Ya existe un vehículo con esa placa.";
                return View(vehiculo);
            }

            vehiculo.fecha_registro_vehiculo = DateTime.Now;

            await _appDBContext.Vehiculos.AddAsync(vehiculo);
            await _appDBContext.SaveChangesAsync();

            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var vehiculo = await _appDBContext.Vehiculos.FindAsync(id);
            if (vehiculo == null)
                return NotFound();

            ViewBag.Clientes = _appDBContext.Clientes.ToList();
            return View(vehiculo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Vehiculo vehiculo)
        {

            var veh = await _appDBContext.Vehiculos.FindAsync(vehiculo.vehiculo_id);
            if (veh == null)
                return NotFound();

            bool placaRepetida = await _appDBContext.Vehiculos
                .AnyAsync(v => v.placa == vehiculo.placa && v.vehiculo_id != vehiculo.vehiculo_id);
            if (placaRepetida)
            {
                ViewBag.Clientes = _appDBContext.Clientes.ToList();
                ViewData["mensaje"] = "Ya existe un vehículo con esa placa.";
                return View(vehiculo);
            }

            veh.modelo = vehiculo.modelo;
            veh.marca = vehiculo.marca;
            veh.placa = vehiculo.placa;
            veh.kilometraje = vehiculo.kilometraje;
            veh.cliente_id = vehiculo.cliente_id;

            await _appDBContext.SaveChangesAsync();
            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var vehiculo = _appDBContext.Vehiculos
                .Include(v => v.cliente)
                .FirstOrDefault(v => v.vehiculo_id == id);

            if (vehiculo == null)
                return NotFound();

            return View(vehiculo);
        }

        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var vehiculo = _appDBContext.Vehiculos.Find(id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            _appDBContext.Vehiculos.Remove(vehiculo);
            _appDBContext.SaveChanges();

            return RedirectToAction(nameof(Mostrar));
        }
    }
}