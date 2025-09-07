using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Ingreso
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idIngreso { get; set; }
        public double costoIngreso { get; set; }
        public DateOnly fechaIngreso { get; set; }
        public DateOnly fechaRegistroIngreso { get; set; }
        public string tipoIngreso { get; set; }
        public string detalleIngreso { get; set; }
        // Relaciones
        public int idUsuario { get; set; }
        [ForeignKey("idUsuario")]
        public Usuario Usuario { get; set; }


    }
}
