using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idCliente { get; set; }
        public string nombre { get; set; }
        
        public string ruc { get; set; }
        public string correoCliente { get; set; }
        public DateOnly fechaRegistroCliente { get; set; }
        public string telefonoCliente { get; set; }
        public string direccionCliente { get; set; }
        public string tipo { get; set; }

        // Relaciones

        public int idUsuario { get; set; }
        [ForeignKey("idUsuario")]
        public Usuario Usuario { get; set; }

        public ICollection<Cotizacion> Cotizaciones { get; set; }
        public ICollection<Vehiculo> Vehiculos { get; set; }

    }
}
