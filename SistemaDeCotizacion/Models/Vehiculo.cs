using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class Vehiculo
    {
        public int vehiculo_id { get; set; }
        public string modelo { get; set; }
        public string marca { get; set; }
        public string placa { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_registro_vehiculo { get; set; }
        public int kilometraje { get; set; }
        public int cliente_id { get; set; }
        public Cliente cliente { get; set; }
    }
}
