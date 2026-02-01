using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
    {
        public void Configure(EntityTypeBuilder<DetalleVenta> builder)
        {
            builder.ToTable("DetallesVenta");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.VentaId)
                .IsRequired();

            builder.Property(d => d.ProductoId)
                .IsRequired();

            builder.Property(d => d.NombreProducto)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(d => d.CodigoBarras)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(d => d.Cantidad)
                .IsRequired();

            builder.Property(d => d.PrecioUnitario)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(d => d.TasaIVA)
                .HasColumnType("decimal(5,4)")
                .IsRequired();

            builder.Property(d => d.GravadoIVA)
                .IsRequired();

            // Relación con Producto (opcional, para navegación)
            builder.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(d => d.VentaId)
                .HasDatabaseName("IX_DetallesVenta_VentaId");

            builder.HasIndex(d => d.ProductoId)
                .HasDatabaseName("IX_DetallesVenta_ProductoId");

            // Propiedades calculadas NO se persisten
            builder.Ignore(d => d.Subtotal);
            builder.Ignore(d => d.Impuestos);
            builder.Ignore(d => d.Total);
        }
    }
}