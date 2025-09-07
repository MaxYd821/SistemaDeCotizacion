using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Cotizacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCotizacion { get; set; }
        public double costoServicioTotal { get; set; }
        public double costoRepuestosTotal { get; set; }
        public string formaPago { get; set; }
        public string estadoCotizacion { get; set; }
        public int tiempoEntrega { get; set; }

        // Relaciones
        public int idCliente { get; set; }
        [ForeignKey("idCliente")]
        public Cliente Cliente { get; set; }

        public ICollection<DetalleRepuesto> detalleRepuesto { get; set; }
        public ICollection<DetalleServicio> detalleServicio { get; set; }

    }
}
