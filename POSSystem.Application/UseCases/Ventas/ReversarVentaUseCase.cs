using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Ventas
{
    /// <summary>
    /// Caso de uso: Reversar una venta ya pagada (OPERACIÓN CRÍTICA).
    /// Cancela venta, devuelve inventario, registra salida de caja.
    /// </summary>
    public class ReversarVentaUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReversarVentaUseCase> _logger;

        public ReversarVentaUseCase(
            IUnitOfWork unitOfWork,
            ILogger<ReversarVentaUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResultadoOperacion> ExecuteAsync(
            Guid ventaId,
            string motivo,
            Guid usuarioId)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando reverso de venta {VentaId}. Motivo: {Motivo}",
                    ventaId, motivo);

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    return ResultadoOperacion.Error(
                        "Debe proporcionar un motivo de cancelación",
                        "MOTIVO_REQUERIDO");
                }

                await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    // PASO 1: Cargar venta con todos sus datos
                    var venta = await _unitOfWork.Ventas.GetByIdAsync(ventaId);
                    if (venta == null)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion.Error(
                            "Venta no encontrada",
                            "VENTA_NO_ENCONTRADA");
                    }

                    // PASO 2: Validar estado
                    if (venta.Estado != EstadoVenta.Pagada)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion.Error(
                            "Solo se pueden reversar ventas pagadas",
                            "VENTA_NO_PAGADA");
                    }

                    // PASO 3: Reversar la venta en el dominio
                    var resultadoReversar = venta.Reversar(motivo);
                    if (resultadoReversar.IsFailure)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion.Error(resultadoReversar.Error);
                    }

                    await _unitOfWork.Ventas.UpdateAsync(venta);

                    // PASO 4: Devolver inventario
                    foreach (var detalle in venta.Detalles)
                    {
                        var inventario = await _unitOfWork.Inventarios
                            .GetByProductoIdWithLockAsync(detalle.ProductoId);

                        if (inventario != null)
                        {
                            // Incrementar stock
                            var resultadoIncremento = inventario.IncrementarStock(detalle.Cantidad);
                            if (resultadoIncremento.IsFailure)
                            {
                                _logger.LogWarning(
                                    "No se pudo incrementar stock del producto {ProductoId}: {Error}",
                                    detalle.ProductoId, resultadoIncremento.Error);
                            }
                            else
                            {
                                await _unitOfWork.Inventarios.UpdateAsync(inventario);

                                // Registrar movimiento de inventario
                                var movimientoInventario = MovimientoInventario.Crear(
                                    detalle.ProductoId,
                                    TipoMovimientoInventario.Devolucion,
                                    detalle.Cantidad,
                                    inventario.StockFisico - detalle.Cantidad,
                                    inventario.StockFisico,
                                    $"Devolución por cancelación - Folio: {venta.Folio.Valor}",
                                    usuarioId,
                                    venta.Folio.Valor);

                                if (movimientoInventario.IsSuccess)
                                {
                                    await _unitOfWork.MovimientosInventario
                                        .AddAsync(movimientoInventario.Value);
                                }
                            }
                        }
                    }

                    // PASO 5: Registrar salida de caja (reverso del movimiento de venta)
                    var caja = await _unitOfWork.Cajas.GetCajaAbiertaAsync();
                    if (caja != null)
                    {
                        var resultadoCancelacion = caja.RegistrarCancelacion(
                            venta.Total,
                            venta.Folio.Valor,
                            usuarioId);

                        if (resultadoCancelacion.IsFailure)
                        {
                            _logger.LogWarning(
                                "No se pudo registrar cancelación en caja: {Error}",
                                resultadoCancelacion.Error);
                        }
                        else
                        {
                            await _unitOfWork.Cajas.UpdateAsync(caja);
                        }
                    }
                    else
                    {
                        _logger.LogWarning(
                            "No hay caja abierta para registrar cancelación de venta {Folio}",
                            venta.Folio.Valor);
                    }

                    // PASO 6: Confirmar transacción
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Venta {Folio} reversada exitosamente. Total devuelto: {Total:C}",
                        venta.Folio.Valor, venta.Total);

                    return ResultadoOperacion.Exito(
                        $"Venta {venta.Folio.Valor} reversada exitosamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en transacción de reverso");
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico reversando venta");
                return ResultadoOperacion.Error(
                    $"Error al reversar venta: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}