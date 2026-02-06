using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace POSSystem.Infrastructure.Persistence.Context
{
    public class POSDbContextFactory : IDesignTimeDbContextFactory<POSDbContext>
    {
        public POSDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<POSDbContext>();

            // Usar ruta absoluta para desarrollo
            optionsBuilder.UseSqlite("Data Source=C:\\Temp\\possystem.db");

            return new POSDbContext(optionsBuilder.Options);
        }
    }
}