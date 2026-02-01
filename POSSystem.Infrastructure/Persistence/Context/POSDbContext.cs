using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Context
{
    /// <summary>
    /// Contexto de base de datos para el sistema POS.
    /// Configura todas las entidades y sus relaciones.
    /// </summary>
    public class POSDbContext : DbContext
    {
        public POSDbContext(DbContextOptions<POSDbContext> options) : base(options)
        {
        }

        // DbSets - Entidades principales
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<ReservaInventario> ReservasInventario { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas las configuraciones del ensamblado
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(POSDbContext).Assembly);

            // Configuraciones globales
            ConfigurarDecimales(modelBuilder);
            ConfigurarCascadeDelete(modelBuilder);
        }

        /// <summary>
        /// Configura la precisión de los campos decimales (dinero).
        /// </summary>
        private void ConfigurarDecimales(ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                // Configurar precisión: 18 dígitos totales, 2 decimales
                property.SetColumnType("decimal(18,2)");
            }
        }

        /// <summary>
        /// Deshabilita cascade delete para prevenir eliminaciones accidentales.
        /// </summary>
        private void ConfigurarCascadeDelete(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        /// <summary>
        /// Sobrescribe SaveChanges para actualizar automáticamente fechas.
        /// </summary>
        public override int SaveChanges()
        {
            ActualizarFechasModificacion();
            return base.SaveChanges();
        }

        /// <summary>
        /// Sobrescribe SaveChangesAsync para actualizar automáticamente fechas.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ActualizarFechasModificacion();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Actualiza UltimaModificacion en entidades modificadas.
        /// </summary>
        private void ActualizarFechasModificacion()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is EntityBase &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (EntityBase)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    // FechaCreacion ya se establece en el constructor de EntityBase
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(EntityBase.UltimaModificacion)).CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}