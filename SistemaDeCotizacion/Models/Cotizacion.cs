using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class Cotizacion
    {
        public int cotizacion_id { get; set; }
        public Double costo_servicio_total { get; set; }
        public Double costo_repuesto_total { get; set; }
        public string formaPago { get; set; }
        public string estado_cotizacion { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_cotizacion { get; set; }
        public int tiempoEntrega { get; set; }
        public string trabajador { get; set; }
        public int? cliente_id { get; set; }
        public Cliente cliente { get; set; }
        public ICollection<DetalleServicio> servicios { get; set; }
        public ICollection<DetalleRepuesto> repuestos { get; set; }
    }
}
