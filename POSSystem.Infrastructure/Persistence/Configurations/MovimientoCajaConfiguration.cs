using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
    {
        public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
        {
            builder.ToTable("MovimientosCaja");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.CajaId)
                .IsRequired();

            builder.Property(m => m.TipoMovimiento)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(m => m.Monto)
                .HasColumnType("decimal(18,2)")
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

            // Índices
            builder.HasIndex(m => m.CajaId)
                .HasDatabaseName("IX_MovimientosCaja_CajaId");

            builder.HasIndex(m => m.TipoMovimiento)
                .HasDatabaseName("IX_MovimientosCaja_TipoMovimiento");

            builder.HasIndex(m => m.FechaMovimiento)
                .HasDatabaseName("IX_MovimientosCaja_FechaMovimiento");

            builder.HasIndex(m => m.Referencia)
                .HasDatabaseName("IX_MovimientosCaja_Referencia");
        }
    }
}