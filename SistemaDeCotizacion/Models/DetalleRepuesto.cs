using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class DetalleRepuesto
    {
        public int detalleRepuesto_id { get; set; }
        public int cantidad_rep { get; set; }
        public Double valor_venta { get; set; }
        public int repuesto_id { get; set; }
        public Repuesto repuesto { get; set; }
        public int cotizacion_id { get; set; }
        public Cotizacion cotizacion { get; set; }
    }
}
