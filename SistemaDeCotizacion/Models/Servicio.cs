using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class Servicio
    {
        public int servicio_id { get; set; }
        public string nombre_servicio { get; set; }
        public Double precio { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_registro_servicio { get; set; }
        public ICollection<DetalleServicio> detalle_servicio { get; set; }
    }
}
