
using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Models;

namespace SistemaDeCotizacion.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }
        // DbSets for each model
        public DbSet<Rol>Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleRepuesto> DetalleRepuestos { get; set; }
        public DbSet<DetalleServicio> DetalleServicios { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure relationships and constraints if needed
            modelBuilder.Entity<Usuario>()
                .HasOne(u=> u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.idRol);
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.clientes)
                .HasForeignKey(c => c.idUsuario);
            modelBuilder.Entity<Ingreso>()
                .HasOne(i => i.Usuario)
                .WithMany(u => u.ingresos)
                .HasForeignKey(i => i.idUsuario);
            modelBuilder.Entity<Vehiculo>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Vehiculos)
                .HasForeignKey(v => v.idCliente);

            modelBuilder.Entity<Cotizacion>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Cotizaciones )
                .HasForeignKey(c => c.idCliente);
            modelBuilder.Entity<DetalleServicio>()
                .HasOne(ds => ds.Servicio)
                .WithMany(s => s.detalleServicio)
                .HasForeignKey(ds => ds.idServicio);
            modelBuilder.Entity<DetalleServicio>()
                .HasOne(ds => ds.Cotizacion)
                .WithMany(c => c.detalleServicio)
                .HasForeignKey(ds => ds.idCotizacion);
            modelBuilder.Entity<DetalleRepuesto>()
                .HasOne(dr => dr.Repuesto)
                .WithMany(r => r.detalleRepuesto)
                .HasForeignKey(dr => dr.idRepuesto);
            modelBuilder.Entity<DetalleRepuesto>()
                .HasOne(dr => dr.Cotizacion)
                .WithMany(c => c.detalleRepuesto)
                .HasForeignKey(dr => dr.idCotizacion);

            modelBuilder.Entity<Repuesto>(tb =>
            {
               tb.Property(r => r.precioRepuesto)
                .HasPrecision(10,2);
            });
            modelBuilder.Entity<Servicio>(tb =>
            {
                tb.Property(r => r.precioServicio)
                 .HasPrecision(10, 2);
            });
            modelBuilder.Entity<Ingreso>(tb =>
            {
                tb.Property(r => r.costoIngreso)
                 .HasPrecision(10, 2);
            });
            modelBuilder.Entity<Cotizacion>(tb =>
            {
                tb.Property(r => r.costoRepuestosTotal)
                 .HasPrecision(10, 2);
                tb.Property(r => r.costoServicioTotal)
                    .HasPrecision(10, 2);
            });
        }
    }
}
