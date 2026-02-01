using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configuración de la entidad Venta (Aggregate Root).
    /// </summary>
    public class VentaConfiguration : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.ToTable("Ventas");

            builder.HasKey(v => v.Id);

            // Configurar Value Object Folio
            builder.OwnsOne(v => v.Folio, folio =>
            {
                folio.Property(f => f.Valor)
                    .HasColumnName("Folio")
                    .HasMaxLength(20)
                    .IsRequired();

                // Índice único
                folio.HasIndex(f => f.Valor)
                    .IsUnique()
                    .HasDatabaseName("IX_Ventas_Folio");
            });

            builder.Property(v => v.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(v => v.UsuarioId)
                .IsRequired();

            builder.Property(v => v.TasaIVA)
                .HasColumnType("decimal(5,4)")
                .IsRequired()
                .HasDefaultValue(0.16m);

            builder.Property(v => v.MotivoCancelacion)
                .HasMaxLength(500);

            builder.Property(v => v.FechaPago)
                .IsRequired(false);

            builder.Property(v => v.FechaCancelacion)
                .IsRequired(false);

            // Configurar concurrencia optimista
            builder.Property(v => v.RowVersion)
                .IsRowVersion();

            // Relaciones
            builder.HasMany(v => v.Detalles)
                .WithOne()
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade); // Los detalles SÍ se eliminan con la venta

            builder.HasMany(v => v.Pagos)
                .WithOne()
                .HasForeignKey(p => p.VentaId)
                .OnDelete(DeleteBehavior.Cascade); // Los pagos SÍ se eliminan con la venta

            // Índices
            builder.HasIndex(v => v.Estado)
                .HasDatabaseName("IX_Ventas_Estado");

            builder.HasIndex(v => v.FechaCreacion)
                .HasDatabaseName("IX_Ventas_FechaCreacion");

            builder.HasIndex(v => v.UsuarioId)
                .HasDatabaseName("IX_Ventas_UsuarioId");

            // Propiedades calculadas NO se persisten
            builder.Ignore(v => v.Subtotal);
            builder.Ignore(v => v.Impuestos);
            builder.Ignore(v => v.Total);
            builder.Ignore(v => v.TotalPagado);
            builder.Ignore(v => v.CambioTotal);
            builder.Ignore(v => v.CantidadProductos);
        }
    }
}