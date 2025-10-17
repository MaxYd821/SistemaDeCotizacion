using Microsoft.EntityFrameworkCore;
using SistemaDeCotizacion.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SistemaDeCotizacion.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {

        }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<Repuesto> Repuestos { get; set; }
        public DbSet<DetalleRepuesto> DetalleRepuestos { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<DetalleServicio> DetalleServicios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Usuario>(tb =>
            {
                tb.HasKey(col => col.usuario_id);
                tb.Property(col => col.usuario_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.nombre).HasMaxLength(50);
                tb.Property(col => col.apellido).HasMaxLength(50);
                tb.Property(col => col.num_cel).HasMaxLength(20);
                tb.Property(col => col.dni).HasMaxLength(8);
                tb.Property(col => col.estado).HasMaxLength(50);
                tb.Property(col => col.correo).HasMaxLength(50);
                tb.Property(col => col.password).HasMaxLength(50);
                tb.HasIndex(col => col.correo).IsUnique();
                tb.HasIndex(col => col.dni).IsUnique();
                tb.HasOne(u => u.rol)
                    .WithMany(r => r.usuarios)
                    .HasForeignKey(u => u.rol_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Rol>(tb =>
            {
                tb.HasKey(col => col.rol_id);
                tb.Property(col => col.rol_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.rol_nombre).HasMaxLength(50);
                tb.Property(col => col.rol_descripcion).HasMaxLength(200);
            });

            modelBuilder.Entity<Ingreso>(tb =>
            {
                tb.HasKey(col => col.ingreso_id);
                tb.Property(col => col.ingreso_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.costo_ingreso).HasPrecision(10, 2);
                tb.Property(col => col.tipo_ingreso).HasMaxLength(50);
                tb.Property(col => col.detalle_ingreso).HasMaxLength(250);
                tb.HasOne(u => u.usuario)
                    .WithMany(r => r.ingresos)
                    .HasForeignKey(u => u.usuario_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cliente>(tb =>
            {
                tb.HasKey(col => col.cliente_id);
                tb.Property(col => col.cliente_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.nombre_cliente).HasMaxLength(50);
                tb.HasIndex(col => col.correo_cliente).IsUnique();
                tb.HasIndex(col => col.ruc).IsUnique();
                tb.Property(col => col.telefono_cliente).HasMaxLength(10);
                tb.Property(col => col.direccion_cliente).HasMaxLength(50);
                tb.Property(col => col.tipo).HasMaxLength(30);
            });

            modelBuilder.Entity<Vehiculo>(tb =>
            {
                tb.HasKey(col => col.vehiculo_id);
                tb.Property(col => col.vehiculo_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.modelo).HasMaxLength(50);
                tb.Property(col => col.marca).HasMaxLength(50);
                tb.Property(col => col.placa).HasMaxLength(7);
                tb.HasIndex(col => col.placa).IsUnique();
                tb.Property(col => col.kilometraje);
                tb.HasOne(u => u.cliente)
                    .WithMany(r => r.vehiculos)
                    .HasForeignKey(u => u.cliente_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cotizacion>(tb =>
            {
                tb.HasKey(col => col.cotizacion_id);
                tb.Property(col => col.cotizacion_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.costo_repuesto_total).HasPrecision(10, 2);
                tb.Property(col => col.costo_servicio_total).HasPrecision(10, 2);
                tb.Property(col => col.formaPago).HasMaxLength(50);
                tb.Property(col => col.tiempoEntrega);
                tb.HasOne(co => co.cliente)
                    .WithMany(cl => cl.cotizaciones)
                    .HasForeignKey(co => co.cliente_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Repuesto>(tb =>
            {
                tb.HasKey(col => col.repuesto_id);
                tb.Property(col => col.repuesto_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.codigo_rep).HasMaxLength(10);
                tb.Property(col => col.descripcion).HasMaxLength(60);
                tb.Property(col => col.medida_rep).HasMaxLength(20);
                tb.Property(col => col.precio_und).HasPrecision(8, 2);
            });

            modelBuilder.Entity<DetalleRepuesto>(tb =>
            {
                tb.HasKey(col => col.detalleRepuesto_id);
                tb.Property(col => col.detalleRepuesto_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.cantidad_rep);
                tb.Property(col => col.valor_venta).HasPrecision(8, 2);
                tb.HasOne(u => u.repuesto)
                    .WithMany(r => r.detalle_repuesto)
                    .HasForeignKey(u => u.repuesto_id)
                    .OnDelete(DeleteBehavior.Restrict);
                tb.HasOne(u => u.cotizacion)
                    .WithMany(r => r.repuestos)
                    .HasForeignKey(u => u.cotizacion_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Servicio>(tb =>
            {
                tb.HasKey(col => col.servicio_id);
                tb.Property(col => col.servicio_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.Property(col => col.nombre_servicio).HasMaxLength(50);
                tb.Property(col => col.precio).HasPrecision(8, 2);
            });

            modelBuilder.Entity<DetalleServicio>(tb =>
            {
                tb.HasKey(col => col.detalleServicio_id);
                tb.Property(col => col.detalleServicio_id).UseIdentityColumn().ValueGeneratedOnAdd();
                tb.HasOne(u => u.servicio)
                    .WithMany(r => r.detalle_servicio)
                    .HasForeignKey(u => u.servicio_id)
                    .OnDelete(DeleteBehavior.Restrict);
                tb.HasOne(u => u.cotizacion)
                    .WithMany(r => r.servicios)
                    .HasForeignKey(u => u.cotizacion_id)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Rol>().ToTable("Rol");
            modelBuilder.Entity<Cliente>().ToTable("Cliente");
            modelBuilder.Entity<Ingreso>().ToTable("Ingreso");
            modelBuilder.Entity<Vehiculo>().ToTable("Vehiculo");
            modelBuilder.Entity<Cotizacion>().ToTable("Cotizacion");
            modelBuilder.Entity<Repuesto>().ToTable("Repuesto");
            modelBuilder.Entity<DetalleRepuesto>().ToTable("DetalleRepuesto");
            modelBuilder.Entity<Servicio>().ToTable("Servicio");
            modelBuilder.Entity<DetalleServicio>().ToTable("DetalleServicio");
        }
    }
}
