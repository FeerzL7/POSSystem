using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
    {
        public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
        {
            builder.ToTable("MovimientosInventario");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.ProductoId)
                .IsRequired();

            builder.Property(m => m.TipoMovimiento)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(m => m.Cantidad)
                .IsRequired();

            builder.Property(m => m.StockAnterior)
                .IsRequired();

            builder.Property(m => m.StockPosterior)
                .IsRequired();

            builder.Property(m => m.Concepto)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(m => m.Referencia)
                .HasMaxLength(100);

            builder.Property(m => m.UsuarioId)
                .IsRequired();

            builder.Property(m => m.FechaMovimiento)
                .IsRequired();

            // Relación con Producto
            builder.HasOne(m => m.Producto)
                .WithMany()
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(m => m.ProductoId)
                .HasDatabaseName("IX_MovimientosInventario_ProductoId");

            builder.HasIndex(m => m.TipoMovimiento)
                .HasDatabaseName("IX_MovimientosInventario_TipoMovimiento");

            builder.HasIndex(m => m.FechaMovimiento)
                .HasDatabaseName("IX_MovimientosInventario_FechaMovimiento");

            builder.HasIndex(m => m.Referencia)
                .HasDatabaseName("IX_MovimientosInventario_Referencia");
        }
    }
}