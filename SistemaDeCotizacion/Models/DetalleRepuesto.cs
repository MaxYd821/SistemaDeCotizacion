using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class DetalleRepuesto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleRepuesto { get; set; }
        public DateOnly fechaDetalleRepuesto { get; set; }
        public int cantidadRepuesto { get; set; }
        public double valorVenta { get; set; }

        // Relaciones
        public int idRepuesto { get; set; }
        [ForeignKey("idRepuesto")]
        public Repuesto Repuesto { get; set; }
        public int idCotizacion { get; set; }
        [ForeignKey("idCotizacion")]
        public Cotizacion Cotizacion { get; set; }

    }
}
