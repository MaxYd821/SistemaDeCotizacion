using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaDeCotizacion.Models
{
    public class Vehiculo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idVehiculo { get; set; }
        public string modelo { get; set; }
        public string marca { get; set; }
        public string placa { get; set; }
        public int kilometraje { get; set; }

        // Relaciones
        public int idCliente { get; set; }
        [ForeignKey("idCliente")]
        public Cliente Cliente { get; set; }
    }
}
