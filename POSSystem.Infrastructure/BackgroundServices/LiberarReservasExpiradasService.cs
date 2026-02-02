using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo que libera automáticamente reservas de inventario expiradas.
    /// Ejecuta cada 1 minuto.
    /// </summary>
    public class LiberarReservasExpiradasService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LiberarReservasExpiradasService> _logger;
        private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(1);

        public LiberarReservasExpiradasService(
            IServiceProvider serviceProvider,
            ILogger<LiberarReservasExpiradasService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Servicio de liberación de reservas iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await LiberarReservasExpiradas();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al liberar reservas expiradas");
                }

                await Task.Delay(_intervalo, stoppingToken);
            }

            _logger.LogInformation("Servicio de liberación de reservas detenido");
        }

        private async Task LiberarReservasExpiradas()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                // Obtener reservas expiradas
                var reservasExpiradas = await unitOfWork.ReservasInventario.GetReservasExpiradasAsync();

                if (!reservasExpiradas.Any())
                    return;

                _logger.LogInformation(
                    "Liberando {Count} reservas expiradas",
                    reservasExpiradas.Count());

                await unitOfWork.BeginTransactionAsync();

                foreach (var reserva in reservasExpiradas)
                {
                    // Obtener inventario
                    var inventario = await unitOfWork.Inventarios.GetByProductoIdAsync(reserva.ProductoId);

                    if (inventario != null)
                    {
                        // Liberar la reserva
                        var resultado = inventario.LiberarReserva(reserva.Cantidad);
                        if (resultado.IsSuccess)
                        {
                            await unitOfWork.Inventarios.UpdateAsync(inventario);
                        }
                        else
                        {
                            _logger.LogWarning(
                                "No se pudo liberar reserva {ReservaId}: {Error}",
                                reserva.Id, resultado.Error);
                        }
                    }

                    // Marcar reserva como expirada
                    var resultadoExpirar = reserva.Expirar();
                    if (resultadoExpirar.IsSuccess)
                    {
                        await unitOfWork.ReservasInventario.UpdateAsync(reserva);
                    }
                }

                await unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "{Count} reservas liberadas exitosamente",
                    reservasExpiradas.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en transacción de liberación de reservas");
                await unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}