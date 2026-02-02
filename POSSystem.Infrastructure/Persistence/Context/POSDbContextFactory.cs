using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POSSystem.Infrastructure.Persistence.Context
{
    /// <summary>
    /// Factory para crear DbContext en tiempo de diseño (migraciones).
    /// </summary>
    public class POSDbContextFactory : IDesignTimeDbContextFactory<POSDbContext>
    {
        public POSDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<POSDbContext>();

            // Conexión SQLite por defecto para migraciones
            optionsBuilder.UseSqlite("Data Source=possystem.db");

            return new POSDbContext(optionsBuilder.Options);
        }
    }
}