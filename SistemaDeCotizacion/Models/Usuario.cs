using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idUsuario { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string dni { get; set; }
        public string telefono { get; set; }
        public DateOnly fechaRegistro { get; set; }
        public DateOnly fechaNacimineto { get; set; }
        public string estado { get; set; }
        public string email { get; set; }
        public string password { get; set; }


        // Relaciones
        public int idRol { get; set; }
        [ForeignKey("idRol")]
        public Rol Rol { get; set; }

        public ICollection<Cliente> clientes { get; set; }
        public ICollection<Ingreso> ingresos { get; set; }
    }
}
