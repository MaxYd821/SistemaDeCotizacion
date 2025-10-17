using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class Cliente
    {
        public int cliente_id { get; set; }
        public string nombre_cliente { get; set; }
        public string correo_cliente { get; set; }
        [RegularExpression(@"^\d{8}$|^\d{11}$", ErrorMessage = "El RUC debe tener 11 dígitos y el DNI 8 dígitos.")]
        public string ruc { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_registro_cliente { get; set; }
        public string telefono_cliente { get; set; }
        public string direccion_cliente { get; set; }
        public string tipo { get; set; }
        public ICollection<Vehiculo> vehiculos { get; set; } = new List<Vehiculo>();
        public ICollection<Cotizacion> cotizaciones { get; set; } = new List<Cotizacion>();
    }
}
