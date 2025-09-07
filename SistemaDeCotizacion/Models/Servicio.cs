using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Servicio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idServicio { get; set; }
        public string nombreServicio { get; set; }
        public double precioServicio { get; set; }
        public DateOnly fechaRegistroServicio { get; set; }

        // Relaciones
        public ICollection<DetalleServicio> detalleServicio { get; set; }
    }
}
