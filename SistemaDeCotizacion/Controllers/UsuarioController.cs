using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Data;
using SistemaDeCotizacion.Models;
using SistemaDeCotizacion.ViewModels;

namespace SistemaDeCotizacion.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDBContext _appDBContext;
        public UsuarioController(AppDBContext appDBContext)
        {
            _appDBContext = appDBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Mostrar(string busqueda = null, int? mes = null, int? anio = null)
        {
            var query = _appDBContext.Usuarios
                .Include(u => u.rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(u =>
                    u.nombre.Contains(busqueda) ||
                    u.apellido.Contains(busqueda) ||
                    u.dni.Contains(busqueda) ||
                    u.rol.rol_nombre.Contains(busqueda)
                );
            }

            if (mes.HasValue)
            {
                query = query.Where(u => u.fecha_registro.Month == mes.Value);
            }

            if (anio.HasValue)
            {
                query = query.Where(u => u.fecha_registro.Year == anio.Value);
            }

            var usuarios = await query
                .OrderByDescending(u => u.fecha_registro)
                .ToListAsync();

            ViewBag.MesSeleccionado = mes;
            ViewBag.AnioSeleccionado = anio;

            ViewBag.Meses = Enumerable.Range(1, 12)
                .Select(i => new SelectListItem
                {
                    Value = i.ToString(),
                    Text = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i),
                    Selected = (i == mes)
                })
                .ToList();

            return View(usuarios);
        }

        [HttpGet]
        public async Task<IActionResult> Nuevo()
        {
            ViewBag.Roles = _appDBContext.Roles.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Nuevo(RegistroVM model)
        {
            if (await _appDBContext.Usuarios.AnyAsync(u => u.correo == model.correo))
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Correo ya registrado.";
                return View(model);
            }

            if (await _appDBContext.Usuarios.AnyAsync(u => u.dni == model.dni))
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "DNI ya registrado.";
                return View(model);
            }

            var edad = DateTime.Today.Year - model.fecha_nacimiento.Year;
            if (model.fecha_nacimiento > DateTime.Today.AddYears(-edad)) edad--;
            if (edad < 18)
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Debes tener al menos 18 años.";
                return View(model);
            }
            if (model.password!=model.confirmarpassword)
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Las contraseñas no coinciden";
                return View(model);
            }

            var usuario = new Usuario
            {
                nombre = model.nombre,
                apellido = model.apellido,
                num_cel = model.num_cel,
                dni = model.dni,
                fecha_registro = DateTime.Now,
                fecha_nacimiento = model.fecha_nacimiento,
                estado = model.estado,
                correo = model.correo,
                password = model.password,
                rol_id = model.rol_id
            };

            await _appDBContext.Usuarios.AddAsync(usuario);
            await _appDBContext.SaveChangesAsync();
            TempData["mensaje"] = "Usuario creado exitosamente.";

            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _appDBContext.Usuarios.FindAsync(id);
            if (usuario == null)
                return NotFound();

            var model = new EditarUsuarioVM
            {
                usuario_id = usuario.usuario_id,
                nombre = usuario.nombre,
                apellido = usuario.apellido,
                num_cel = usuario.num_cel,
                dni = usuario.dni,
                fecha_nacimiento = usuario.fecha_nacimiento,
                estado = usuario.estado,
                correo = usuario.correo,
                password = usuario.password,
                rol_id = usuario.rol_id
            };

            ViewBag.Roles = _appDBContext.Roles.ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(EditarUsuarioVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Revisa los datos ingresados.";
                return View(model);
            }

            var usuario = await _appDBContext.Usuarios.FindAsync(model.usuario_id);
            if (usuario == null)
                return NotFound();

            bool correoRepetido = await _appDBContext.Usuarios
                .AnyAsync(u => u.correo == model.correo && u.usuario_id != model.usuario_id);
            if (correoRepetido)
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Ya existe un usuario con ese correo.";
                return View(model);
            }

            bool dniRepetido = await _appDBContext.Usuarios
                .AnyAsync(u => u.dni == model.dni && u.usuario_id != model.usuario_id);
            if (dniRepetido)
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "Ya existe un usuario con ese DNI.";
                return View(model);
            }

            var edad = DateTime.Today.Year - model.fecha_nacimiento.Year;
            if (model.fecha_nacimiento > DateTime.Today.AddYears(-edad)) edad--;
            if (edad < 18)
            {
                ViewBag.Roles = _appDBContext.Roles.ToList();
                ViewData["mensaje"] = "El usuario debe tener al menos 18 años.";
                return View(model);
            }

            usuario.nombre = model.nombre;
            usuario.apellido = model.apellido;
            usuario.num_cel = model.num_cel;
            usuario.dni = model.dni;
            usuario.fecha_nacimiento = model.fecha_nacimiento;
            usuario.estado = model.estado;
            usuario.correo = model.correo;
            usuario.password = model.password;
            usuario.rol_id = model.rol_id;

            await _appDBContext.SaveChangesAsync();
            TempData["mensaje"] = "Usuario editado exitosamente.";
            return RedirectToAction(nameof(Mostrar));
        }

        [HttpGet]
        public IActionResult Eliminar(int id)
        {
            var idUsuario = _appDBContext.Usuarios.Find(id);
            return View(idUsuario);
        }
        [HttpPost]
        public IActionResult ConfirmacionEliminar(int id)
        {
            var usuario = _appDBContext.Usuarios.Find(id);
            if (usuario == null)
            {
                return NotFound();
            }
            _appDBContext.Usuarios.Remove(usuario);
            _appDBContext.SaveChanges();

            TempData["mensaje"] = "Usuario eliminado con éxito.";
            return RedirectToAction(nameof(Mostrar));
        }
    }
}
