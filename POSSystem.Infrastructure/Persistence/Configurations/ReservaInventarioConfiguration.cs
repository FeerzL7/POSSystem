using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    public class ReservaInventarioConfiguration : IEntityTypeConfiguration<ReservaInventario>
    {
        public void Configure(EntityTypeBuilder<ReservaInventario> builder)
        {
            builder.ToTable("ReservasInventario");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ProductoId)
                .IsRequired();

            builder.Property(r => r.VentaId)
                .IsRequired();

            builder.Property(r => r.Cantidad)
                .IsRequired();

            builder.Property(r => r.FechaExpiracion)
                .IsRequired();

            builder.Property(r => r.Estado)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(r => r.FechaConfirmacion)
                .IsRequired(false);

            builder.Property(r => r.FechaCancelacion)
                .IsRequired(false);

            builder.Property(r => r.MotivoCancelacion)
                .HasMaxLength(500);

            // Relaciones
            builder.HasOne(r => r.Producto)
                .WithMany()
                .HasForeignKey(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Venta)
                .WithMany()
                .HasForeignKey(r => r.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
            builder.HasIndex(r => r.ProductoId)
                .HasDatabaseName("IX_ReservasInventario_ProductoId");

            builder.HasIndex(r => r.VentaId)
                .HasDatabaseName("IX_ReservasInventario_VentaId");

            builder.HasIndex(r => r.Estado)
                .HasDatabaseName("IX_ReservasInventario_Estado");

            builder.HasIndex(r => r.FechaExpiracion)
                .HasDatabaseName("IX_ReservasInventario_FechaExpiracion");
        }
    }
}