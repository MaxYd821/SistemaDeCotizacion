using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Rol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int rol_id { get; set; }
        public string rol_nombre { get; set; }
        public string rol_descripcion { get; set; }

        // Relaciones
        public ICollection<Usuario> Usuarios { get; set; }

    }
}
