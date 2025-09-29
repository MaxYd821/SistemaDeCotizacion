using Microsoft.AspNetCore.Mvc;
using SistemaDeCotizacion.Data;

namespace SistemaDeCotizacion.Controllers
{
    public class VehiculoController : Controller
    {
        private readonly AppDBContext _context;

        public VehiculoController(AppDBContext context)
        {
            _context = context;
        }
    }
}
