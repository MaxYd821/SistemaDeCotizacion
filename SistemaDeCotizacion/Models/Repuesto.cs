using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Repuesto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idRepuesto { get; set; }
        public string codigoRepuesto { get; set; }
        public string medidaRepuesto { get; set; }
        public double precioRepuesto { get; set; }
        public DateOnly fechaRegistroRepuesto { get; set; }

        // Relaciones
        public ICollection<DetalleRepuesto> detalleRepuesto { get; set; }
    }
}
