using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POSSystem.Application.Interfaces;
using POSSystem.Domain.DomainServices;
using POSSystem.Domain.Interfaces;
using POSSystem.Infrastructure.BackgroundServices;
using POSSystem.Infrastructure.Persistence.Context;
using POSSystem.Infrastructure.Persistence.Repositories;
using POSSystem.Infrastructure.Persistence.UnitOfWork;
using POSSystem.Infrastructure.Services;

namespace POSSystem.Infrastructure
{
    /// <summary>
    /// Configuración de inyección de dependencias para Infrastructure.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext con SQLite
            services.AddDbContext<POSDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=possystem.db";

                options.UseSqlite(connectionString);

                // Configuración adicional para desarrollo
#if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
#endif
            });

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositorios (no es necesario registrarlos individualmente si se usa UnitOfWork)
            // Pero los registramos por si se necesitan directamente
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<IInventarioRepository, InventarioRepository>();
            services.AddScoped<IReservaInventarioRepository, ReservaInventarioRepository>();
            services.AddScoped<ICajaRepository, CajaRepository>();
            services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
            services.AddScoped<IFolioRepository, FolioRepository>();

            // Domain Services
            services.AddScoped<GeneradorFolio>();

            // Application Services
            services.AddScoped<ITicketService, TicketService>();

            // Background Services (comentados por defecto, activar según necesidad)
            // services.AddHostedService<LiberarReservasExpiradasService>();
            // services.AddHostedService<LimpiarReservasAntiguasService>();

            return services;
        }
    }
}