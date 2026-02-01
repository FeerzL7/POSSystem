using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    public class PagoConfiguration : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.VentaId)
                .IsRequired();

            builder.Property(p => p.Monto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.TipoPago)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(p => p.FechaPago)
                .IsRequired();

            builder.Property(p => p.Referencia)
                .HasMaxLength(100);

            builder.Property(p => p.Cambio)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0);

            // Índices
            builder.HasIndex(p => p.VentaId)
                .HasDatabaseName("IX_Pagos_VentaId");

            builder.HasIndex(p => p.FechaPago)
                .HasDatabaseName("IX_Pagos_FechaPago");
        }
    }
}