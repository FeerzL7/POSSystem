using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;
using POSSystem.Domain.ValueObjects;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Configuración de la entidad Producto para Entity Framework.
    /// </summary>
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.ToTable("Productos");

            builder.HasKey(p => p.Id);

            // Configurar Value Object CodigoBarras
            builder.OwnsOne(p => p.CodigoBarras, cb =>
            {
                cb.Property(c => c.Valor)
                    .HasColumnName("CodigoBarras")
                    .HasMaxLength(20)
                    .IsRequired();

                // Índice único para búsqueda rápida
                cb.HasIndex(c => c.Valor)
                    .IsUnique()
                    .HasDatabaseName("IX_Productos_CodigoBarras");
            });

            builder.Property(p => p.Nombre)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Descripcion)
                .HasMaxLength(500);

            builder.Property(p => p.Categoria)
                .HasMaxLength(100);

            builder.Property(p => p.PrecioVenta)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.PrecioCosto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Activo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(p => p.GravadoIVA)
                .IsRequired()
                .HasDefaultValue(true);

            // Configurar concurrencia optimista
            builder.Property(p => p.RowVersion)
                .IsRowVersion();

            // Índices
            builder.HasIndex(p => p.Nombre)
                .HasDatabaseName("IX_Productos_Nombre");

            builder.HasIndex(p => p.Categoria)
                .HasDatabaseName("IX_Productos_Categoria");

            builder.HasIndex(p => p.Activo)
                .HasDatabaseName("IX_Productos_Activo");
        }
    }
}