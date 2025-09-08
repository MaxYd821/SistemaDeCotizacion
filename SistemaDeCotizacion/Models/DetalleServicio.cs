using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class DetalleServicio
    {
        public int detalleServicio_id { get; set; }
        public int servicio_id { get; set; }
        public Servicio servicio { get; set; }
        public int cotizacion_id { get; set; }
        public Cotizacion cotizacion { get; set; }
    }
}
