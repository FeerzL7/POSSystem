using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Ventas
{
    /// <summary>
    /// Caso de uso: Cancelar una venta.
    /// Libera reservas de inventario si la venta está abierta.
    /// </summary>
    public class CancelarVentaUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CancelarVentaUseCase> _logger;

        public CancelarVentaUseCase(
            IUnitOfWork unitOfWork,
            ILogger<CancelarVentaUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Cancela una venta que NO ha sido finalizada (estado Abierta o Nueva).
        /// </summary>
        public async Task<ResultadoOperacion> ExecuteAsync(Venta venta, string motivo, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando cancelación de venta {Folio}. Motivo: {Motivo}",
                    venta.Folio.Valor, motivo);

                // Validar estado
                if (venta.Estado == EstadoVenta.Pagada)
                {
                    return ResultadoOperacion.Error(
                        "Use ReversarVentaUseCase para cancelar ventas pagadas",
                        "VENTA_YA_PAGADA");
                }

                if (venta.Estado == EstadoVenta.Cancelada)
                {
                    return ResultadoOperacion.Error(
                        "La venta ya está cancelada",
                        "VENTA_YA_CANCELADA");
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted);

                try
                {
                    // 1. Cancelar la venta en el dominio
                    var resultadoCancelar = venta.Cancelar(motivo);
                    if (resultadoCancelar.IsFailure)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion.Error(resultadoCancelar.Error);
                    }

                    // 2. Liberar reservas de inventario
                    var reservas = await _unitOfWork.ReservasInventario.GetPorVentaIdAsync(venta.Id);

                    foreach (var reserva in reservas.Where(r => r.Estado == EstadoReserva.Activa))
                    {
                        // Obtener inventario
                        var inventario = await _unitOfWork.Inventarios
                            .GetByProductoIdAsync(reserva.ProductoId);

                        if (inventario != null)
                        {
                            // Liberar la reserva
                            var resultadoLiberar = inventario.LiberarReserva(reserva.Cantidad);
                            if (resultadoLiberar.IsSuccess)
                            {
                                await _unitOfWork.Inventarios.UpdateAsync(inventario);
                            }
                        }

                        // Marcar reserva como cancelada
                        var resultadoCancelarReserva = reserva.Cancelar(motivo);
                        if (resultadoCancelarReserva.IsSuccess)
                        {
                            await _unitOfWork.ReservasInventario.UpdateAsync(reserva);
                        }
                    }

                    // 3. NO guardamos la venta en BD si nunca se persistió
                    // Solo actualizamos si ya existía
                    var ventaExistente = await _unitOfWork.Ventas.GetByIdAsync(venta.Id);
                    if (ventaExistente != null)
                    {
                        await _unitOfWork.Ventas.UpdateAsync(venta);
                    }

                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Venta {Folio} cancelada exitosamente",
                        venta.Folio.Valor);

                    return ResultadoOperacion.Exito(
                        $"Venta {venta.Folio.Valor} cancelada exitosamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en transacción de cancelación {Folio}", venta.Folio.Valor);
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico cancelando venta");
                return ResultadoOperacion.Error(
                    $"Error al cancelar venta: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}