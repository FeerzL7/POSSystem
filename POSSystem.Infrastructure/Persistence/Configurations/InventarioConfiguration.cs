using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    public class InventarioConfiguration : IEntityTypeConfiguration<Inventario>
    {
        public void Configure(EntityTypeBuilder<Inventario> builder)
        {
            builder.ToTable("Inventarios");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.ProductoId)
                .IsRequired();

            builder.Property(i => i.StockFisico)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(i => i.CantidadReservada)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(i => i.StockMinimo)
                .IsRequired()
                .HasDefaultValue(10);

            builder.Property(i => i.StockMaximo)
                .IsRequired()
                .HasDefaultValue(1000);

            builder.Property(i => i.UltimaActualizacionStock)
                .IsRequired();

            // Configurar concurrencia optimista (CRÍTICO para evitar race conditions)
            builder.Property(i => i.RowVersion)
                .IsRowVersion();

            // Relación con Producto
            builder.HasOne(i => i.Producto)
                .WithMany()
                .HasForeignKey(i => i.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único: un producto solo puede tener un inventario
            builder.HasIndex(i => i.ProductoId)
                .IsUnique()
                .HasDatabaseName("IX_Inventarios_ProductoId");

            // Propiedades calculadas NO se persisten
            builder.Ignore(i => i.StockDisponible);
            builder.Ignore(i => i.StockBajo);
        }
    }
}