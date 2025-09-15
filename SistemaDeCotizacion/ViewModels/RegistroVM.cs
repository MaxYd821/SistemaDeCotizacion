using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.ViewModels
{
    public class RegistroVM
    {
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string num_cel { get; set; }
        public string dni { get; set; }
        [DataType(DataType.Date)]
        public DateTime fecha_nacimiento { get; set; }
        public string estado { get; set; } = "Activo";
        public string correo { get; set; }
        public string password { get; set; }
        public string confirmarpassword { get; set; }
        public int rol_id { get; set; }
    }
}
