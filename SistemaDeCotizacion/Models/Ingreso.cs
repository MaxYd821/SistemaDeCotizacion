using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class Ingreso
    {
        public int ingreso_id { get; set; }
        public Double costo_ingreso { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_ingreso { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_registro_ingreso { get; set; }
        public string tipo_ingreso { get; set; }
        public string detalle_ingreso { get; set; }
        public int usuario_id { get; set; }
        public Usuario usuario { get; set; }
    }
}
