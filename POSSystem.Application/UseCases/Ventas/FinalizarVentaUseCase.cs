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
    /// Caso de uso: Finalizar venta (OPERACIÓN ATÓMICA CRÍTICA).
    /// Guarda venta, descuenta inventario, registra en caja, confirma reservas.
    /// TODO debe ejecutarse en una transacción o fallar completamente.
    /// </summary>
    public class FinalizarVentaUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FinalizarVentaUseCase> _logger;

        public FinalizarVentaUseCase(
            IUnitOfWork unitOfWork,
            ILogger<FinalizarVentaUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResultadoOperacion<VentaDto>> ExecuteAsync(Venta venta, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation(
                    "Iniciando finalización de venta {Folio}. Usuario: {UsuarioId}",
                    venta.Folio.Valor, usuarioId);

                // ============================================
                // INICIO DE TRANSACCIÓN ATÓMICA
                // ============================================
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable);

                try
                {
                    // PASO 1: Validar estado de la venta
                    if (venta.Estado != EstadoVenta.Pagada)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion<VentaDto>.Error(
                            "Solo se pueden finalizar ventas pagadas",
                            "VENTA_NO_PAGADA");
                    }

                    if (!venta.Detalles.Any())
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion<VentaDto>.Error(
                            "La venta no tiene productos",
                            "VENTA_SIN_PRODUCTOS");
                    }

                    // PASO 2: Obtener caja abierta
                    var caja = await _unitOfWork.Cajas.GetCajaAbiertaAsync();
                    if (caja == null)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion<VentaDto>.Error(
                            "No hay caja abierta",
                            "CAJA_NO_ABIERTA");
                    }

                    // PASO 3: Guardar venta
                    await _unitOfWork.Ventas.AddAsync(venta);

                    // PASO 4: Confirmar reservas y descontar inventario
                    var reservas = await _unitOfWork.ReservasInventario.GetPorVentaIdAsync(venta.Id);

                    foreach (var detalle in venta.Detalles)
                    {
                        // Obtener inventario con lock
                        var inventario = await _unitOfWork.Inventarios
                            .GetByProductoIdWithLockAsync(detalle.ProductoId);

                        if (inventario == null)
                        {
                            await _unitOfWork.RollbackAsync();
                            return ResultadoOperacion<VentaDto>.Error(
                                $"No hay inventario para el producto {detalle.NombreProducto}",
                                "INVENTARIO_NO_ENCONTRADO");
                        }

                        // Confirmar reserva (descuenta stock físico y libera reserva)
                        var resultadoConfirmacion = inventario.ConfirmarReserva(detalle.Cantidad);
                        if (resultadoConfirmacion.IsFailure)
                        {
                            await _unitOfWork.RollbackAsync();
                            return ResultadoOperacion<VentaDto>.Error(
                                resultadoConfirmacion.Error,
                                "ERROR_CONFIRMAR_RESERVA");
                        }

                        await _unitOfWork.Inventarios.UpdateAsync(inventario);

                        // Registrar movimiento de inventario (auditoría)
                        var movimientoInventario = MovimientoInventario.Crear(
                            detalle.ProductoId,
                            TipoMovimientoInventario.Venta,
                            -detalle.Cantidad,
                            inventario.StockFisico + detalle.Cantidad, // Stock anterior
                            inventario.StockFisico, // Stock posterior
                            $"Venta - Folio: {venta.Folio.Valor}",
                            usuarioId,
                            venta.Folio.Valor);

                        if (movimientoInventario.IsSuccess)
                        {
                            await _unitOfWork.MovimientosInventario.AddAsync(movimientoInventario.Value);
                        }

                        // Marcar reserva como confirmada
                        var reserva = reservas.FirstOrDefault(r => r.ProductoId == detalle.ProductoId);
                        if (reserva != null)
                        {
                            reserva.Confirmar();
                            await _unitOfWork.ReservasInventario.UpdateAsync(reserva);
                        }
                    }

                    // PASO 5: Registrar movimiento en caja
                    var resultadoMovimiento = caja.RegistrarVenta(
                        venta.Total,
                        venta.Folio.Valor,
                        usuarioId);

                    if (resultadoMovimiento.IsFailure)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion<VentaDto>.Error(
                            resultadoMovimiento.Error,
                            "ERROR_REGISTRAR_CAJA");
                    }

                    await _unitOfWork.Cajas.UpdateAsync(caja);

                    // PASO 6: Confirmar transacción
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Venta {Folio} finalizada exitosamente. Total: {Total:C}",
                        venta.Folio.Valor, venta.Total);

                    // ============================================
                    // FIN DE TRANSACCIÓN ATÓMICA
                    // ============================================

                    var ventaDto = venta.ToDto();
                    return ResultadoOperacion<VentaDto>.Exito(
                        ventaDto,
                        $"Venta {venta.Folio.Valor} finalizada exitosamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en transacción de venta {Folio}", venta.Folio.Valor);
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico finalizando venta");
                return ResultadoOperacion<VentaDto>.Error(
                    $"Error al finalizar venta: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}
