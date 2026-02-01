using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Persistence.Configurations
{
    public class CajaConfiguration : IEntityTypeConfiguration<Caja>
    {
        public void Configure(EntityTypeBuilder<Caja> builder)
        {
            builder.ToTable("Cajas");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.NumeroCaja)
                .IsRequired();

            builder.Property(c => c.Nombre)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.EstaAbierta)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.FondoInicial)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.SaldoActual)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.FechaApertura)
                .IsRequired(false);

            builder.Property(c => c.FechaCierre)
                .IsRequired(false);

            builder.Property(c => c.UsuarioAperturaId)
                .IsRequired(false);

            builder.Property(c => c.UsuarioCierreId)
                .IsRequired(false);

            builder.Property(c => c.SaldoFinalDeclarado)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(c => c.Diferencia)
                .HasColumnType("decimal(18,2)")
                .IsRequired(false);

            builder.Property(c => c.ObservacionesCierre)
                .HasMaxLength(1000);

            // Configurar concurrencia optimista
            builder.Property(c => c.RowVersion)
                .IsRowVersion();

            // Relaciones
            builder.HasMany(c => c.Movimientos)
                .WithOne(m => m.Caja)
                .HasForeignKey(m => m.CajaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único: número de caja
            builder.HasIndex(c => c.NumeroCaja)
                .IsUnique()
                .HasDatabaseName("IX_Cajas_NumeroCaja");

            builder.HasIndex(c => c.EstaAbierta)
                .HasDatabaseName("IX_Cajas_EstaAbierta");

            // Propiedades calculadas NO se persisten
            builder.Ignore(c => c.TotalVentas);
            builder.Ignore(c => c.TotalCancelaciones);
            builder.Ignore(c => c.TotalRetiros);
            builder.Ignore(c => c.SaldoCalculado);
        }
    }
}