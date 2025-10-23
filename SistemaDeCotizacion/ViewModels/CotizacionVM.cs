using SistemaDeCotizacion.Models;

namespace SistemaDeCotizacion.ViewModels
{
    public class CotizacionVM
    {
        // Datos generales
        public int CotizacionId { get; set; }
        public int? ClienteId { get; set; }
        public int? VehiculoId { get; set; }
        public string formaPago { get; set; }
        public string estado_cotizacion { get; set; } = "Pendiente";
        public int tiempoEntrega { get; set; }
        public string trabajador { get; set; }

        // Listas cargadas desde BD
        public List<Cliente> Clientes { get; set; }
        public List<Vehiculo> Vehiculos { get; set; }
        public List<Servicio> Servicios { get; set; }
        public List<Repuesto> Repuestos { get; set; }

        // Selecciones del formulario
        public List<ServicioSeleccionadoVM> ServiciosSeleccionados { get; set; } = new List<ServicioSeleccionadoVM>();
        public List<RepuestoSeleccionadoVM> RepuestosSeleccionados { get; set; } = new List<RepuestoSeleccionadoVM>();
    }

    public class RepuestoSeleccionadoVM
    {
        public int RepuestoId { get; set; }
        public int Cantidad { get; set; }
        public string codigo_rep { get; set; }
        public string descripcion { get; set; }
        public string medida_rep { get; set; }
        public Double precio_und { get; set; }
    }

    public class ServicioSeleccionadoVM
    {
        public int ServicioId { get; set; }
        public Double precio { get; set; }
        public string nombre_servicio { get; set; }
    }
}
