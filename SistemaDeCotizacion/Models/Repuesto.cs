using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class Repuesto
    {
        public int repuesto_id { get; set; }
        public string codigo_rep { get; set; }
        public string descripcion { get; set; }
        public int stock { get; set; }
        public string medida_rep { get; set; }
        public Double precio_und { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_registro_repuesto { get; set; }
        public ICollection<DetalleRepuesto> detalle_repuesto { get; set; }
    }
}
