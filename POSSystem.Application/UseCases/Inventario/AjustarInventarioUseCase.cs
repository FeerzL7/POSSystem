using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Inventario
{
    /// <summary>
    /// Caso de uso: Ajustar inventario (corrección de stock físico).
    /// </summary>
    public class AjustarInventarioUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AjustarInventarioUseCase> _logger;

        public AjustarInventarioUseCase(
            IUnitOfWork unitOfWork,
            ILogger<AjustarInventarioUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResultadoOperacion> ExecuteAsync(
            Guid productoId,
            int nuevoStock,
            string motivo,
            Guid usuarioId)
        {
            try
            {
                if (nuevoStock < 0)
                {
                    return ResultadoOperacion.Error(
                        "El stock no puede ser negativo",
                        "STOCK_INVALIDO");
                }

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    return ResultadoOperacion.Error(
                        "Debe proporcionar un motivo para el ajuste",
                        "MOTIVO_REQUERIDO");
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var inventario = await _unitOfWork.Inventarios
                        .GetByProductoIdWithLockAsync(productoId);

                    if (inventario == null)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion.Error(
                            "No se encontró inventario para el producto",
                            "INVENTARIO_NO_ENCONTRADO");
                    }

                    var stockAnterior = inventario.StockFisico;

                    // Ajustar stock
                    var resultado = inventario.AjustarStock(nuevoStock, motivo);
                    if (resultado.IsFailure)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion.Error(resultado.Error);
                    }

                    await _unitOfWork.Inventarios.UpdateAsync(inventario);

                    // Registrar movimiento de inventario
                    var cantidad = nuevoStock - stockAnterior;
                    var tipoMovimiento = cantidad >= 0
                        ? TipoMovimientoInventario.Ajuste
                        : TipoMovimientoInventario.Ajuste;

                    var movimiento = Domain.Entities.MovimientoInventario.Crear(
                        productoId,
                        tipoMovimiento,
                        cantidad,
                        stockAnterior,
                        nuevoStock,
                        $"Ajuste de inventario: {motivo}",
                        usuarioId);

                    if (movimiento.IsSuccess)
                    {
                        await _unitOfWork.MovimientosInventario.AddAsync(movimiento.Value);
                    }

                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Inventario ajustado. Producto: {ProductoId}, Stock anterior: {StockAnterior}, Nuevo: {NuevoStock}",
                        productoId, stockAnterior, nuevoStock);

                    return ResultadoOperacion.Exito(
                        $"Inventario ajustado: {stockAnterior} → {nuevoStock}");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ajustando inventario");
                return ResultadoOperacion.Error(
                    $"Error al ajustar inventario: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}