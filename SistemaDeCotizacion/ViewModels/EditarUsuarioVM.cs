using System.ComponentModel.DataAnnotations;

namespace SistemaDeCotizacion.ViewModels
{
    public class EditarUsuarioVM
    {
        public int usuario_id { get; set; }

        [Required]
        public string nombre { get; set; }

        [Required]
        public string apellido { get; set; }

        [Required]
        public string num_cel { get; set; }

        [Required]
        [StringLength(8)]
        public string dni { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime fecha_nacimiento { get; set; }

        [Required]
        public string estado { get; set; }

        [Required]
        public string correo { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public int rol_id { get; set; }
    }
}
