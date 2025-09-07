using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class DetalleServicio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idDetalleServicio { get; set; }
        public DateOnly fechaServicio { get; set; }

        // Relaciones
        public int idServicio { get; set; }
        [ForeignKey("idServicio")]
        public Servicio Servicio { get; set; }
        public int idCotizacion { get; set; }
        [ForeignKey("idCotizacion")]
        public Cotizacion Cotizacion { get; set; }
    }
}
