using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo que limpia reservas antiguas (más de 30 días).
    /// Ejecuta una vez al día.
    /// </summary>
    public class LimpiarReservasAntiguasService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LimpiarReservasAntiguasService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromHours(24);

        public LimpiarReservasAntiguasService(
            IServiceProvider serviceProvider,
            ILogger<LimpiarReservasAntiguasService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de limpieza de reservas antiguas iniciado");

            // Esperar 1 hora antes de la primera ejecución
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await LimpiarReservasAntiguas();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al limpiar reservas antiguas");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("Servicio de limpieza de reservas antiguas detenido");
        }

        private async Task LimpiarReservasAntiguas()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var fechaLimite = DateTime.UtcNow.AddDays(-30);

                await unitOfWork.BeginTransactionAsync();
                await unitOfWork.ReservasInventario.EliminarReservasAntiguasAsync(fechaLimite);
                var resultado = await unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Limpieza de reservas antiguas completada. Registros eliminados: {Count}",
                    resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en transacción de limpieza de reservas");
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}