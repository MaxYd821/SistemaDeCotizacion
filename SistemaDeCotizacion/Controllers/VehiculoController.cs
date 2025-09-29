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
        private readonly AppDBContext _context;

        public VehiculoController(AppDBContext context)
        {
            _context = context;
        }

        // Mostrar todos los vehículos con su cliente
        public async Task<IActionResult> Mostrar()
        {
            var vehiculos = await _context.Vehiculos
                .Include(v => v.cliente)
                .ToListAsync();
            return View(vehiculos);
        }

        // Vista para crear un nuevo vehículo
        [HttpGet]
        public IActionResult Nuevo()
        {
            ViewData["Clientes"] = _context.Clientes.ToList();
            return View();
        }

        // Crear un nuevo vehículo
        [HttpPost]
        public async Task<IActionResult> Nuevo(Vehiculo vehiculo)
        {
            // Validación de placa duplicada
            if (await _context.Vehiculos.AnyAsync(v => v.placa == vehiculo.placa))
            {
                ViewData["mensaje"] = "Placa ya registrada.";
                ViewData["Clientes"] = _context.Clientes.ToList();
                return View(vehiculo);
            }

            var cliente = await _context.Clientes
                .Include(c => c.vehiculos)
                .FirstOrDefaultAsync(c => c.cliente_id == vehiculo.cliente_id);

            if (cliente == null)
            {
                ViewData["mensaje"] = "Cliente no encontrado.";
                ViewData["Clientes"] = _context.Clientes.ToList();
                return View(vehiculo);
            }

            vehiculo.fecha_registro_vehiculo = System.DateTime.Now;
            cliente.vehiculos.Add(vehiculo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Mostrar");
        }

        // Vista para editar vehículo
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
                return NotFound();

            ViewData["Clientes"] = _context.Clientes.ToList();
            return View(vehiculo);
        }

        // Editar vehículo
        [HttpPost]
        public async Task<IActionResult> Editar(Vehiculo vehiculo)
        {
            var existente = await _context.Vehiculos.FindAsync(vehiculo.vehiculo_id);
            if (existente == null)
                return NotFound();

            // Validación de placa duplicada (excluyendo el actual)
            if (await _context.Vehiculos.AnyAsync(v => v.placa == vehiculo.placa && v.vehiculo_id != vehiculo.vehiculo_id))
            {
                ViewData["mensaje"] = "Placa ya registrada.";
                ViewData["Clientes"] = _context.Clientes.ToList();
                return View(vehiculo);
            }

            existente.modelo = vehiculo.modelo;
            existente.marca = vehiculo.marca;
            existente.placa = vehiculo.placa;
            existente.kilometraje = vehiculo.kilometraje;
            existente.cliente_id = vehiculo.cliente_id;

            await _context.SaveChangesAsync();
            return RedirectToAction("Mostrar");
        }

        // Vista para eliminar vehículo
        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var vehiculo = await _context.Vehiculos
                .Include(v => v.cliente)
                .FirstOrDefaultAsync(v => v.vehiculo_id == id);

            if (vehiculo == null)
                return NotFound();

            return View(vehiculo);
        }

        // Confirmar eliminación de vehículo
        [HttpPost]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var vehiculo = await _context.Vehiculos.FindAsync(id);
            if (vehiculo == null)
                return NotFound();

            _context.Vehiculos.Remove(vehiculo);
            await _context.SaveChangesAsync();
            return RedirectToAction("Mostrar");
        }
    }
}