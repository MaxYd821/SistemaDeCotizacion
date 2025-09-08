using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.Models
{
    public class Usuario
    {
        public int usuario_id { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string num_cel { get; set; }
        public string dni { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_registro { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime fecha_nacimiento { get; set; }
        public string estado { get; set; }
        public string correo { get; set; }
        public string password { get; set; }
        public int rol_id { get; set; }
        public Rol rol { get; set; }
        public ICollection<Cliente> clientes { get; set; }
        public ICollection<Ingreso> ingresos { get; set; }
    }
}
