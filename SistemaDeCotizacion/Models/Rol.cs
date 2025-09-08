namespace SistemaDeCotizacion.Models
{
    public class Rol
    {
        public int rol_id { get; set; }
        public string rol_nombre { get; set; }
        public string rol_descripcion { get; set; }
        public ICollection<Usuario> usuarios { get; set; }
    }
}
