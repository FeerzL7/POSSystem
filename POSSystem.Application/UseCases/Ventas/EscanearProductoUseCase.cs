using System;
using System.Threading.Tasks;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Domain.Exceptions;
using POSSystem.Domain.Interfaces;
using POSSystem.Domain.ValueObjects;

namespace POSSystem.Application.UseCases.Ventas
{
    /// <summary>
    /// Caso de uso: Escanear producto y agregarlo a la venta.
    /// Incluye validación de existencia, stock y creación de reserva.
    /// </summary>
    public class EscanearProductoUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public EscanearProductoUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ResultadoOperacion<ProductoDto>> ExecuteAsync(
            string codigoBarras,
            Guid ventaId,
            int cantidad = 1)
        {
            try
            {
                // 1. Validar código de barras
                var resultadoCodigoBarras = CodigoBarras.Crear(codigoBarras);
                if (resultadoCodigoBarras.IsFailure)
                    return ResultadoOperacion<ProductoDto>.Error(resultadoCodigoBarras.Error);

                await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                // 2. Buscar producto
                var producto = await _unitOfWork.Productos.GetByCodigoBarrasAsync(codigoBarras);
                if (producto == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return ResultadoOperacion<ProductoDto>.Error(
                        $"Producto no encontrado: {codigoBarras}",
                        "PRODUCTO_NO_ENCONTRADO");
                }

                // 3. Validar que esté activo
                if (!producto.Activo)
                {
                    await _unitOfWork.RollbackAsync();
                    return ResultadoOperacion<ProductoDto>.Error(
                        $"El producto '{producto.Nombre}' está inactivo",
                        "PRODUCTO_INACTIVO");
                }

                // 4. Obtener inventario con bloqueo (FOR UPDATE)
                var inventario = await _unitOfWork.Inventarios.GetByProductoIdWithLockAsync(producto.Id);
                if (inventario == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return ResultadoOperacion<ProductoDto>.Error(
                        $"No hay inventario para el producto '{producto.Nombre}'",
                        "INVENTARIO_NO_ENCONTRADO");
                }

                // 5. Validar stock disponible
                if (inventario.StockDisponible < cantidad)
                {
                    await _unitOfWork.RollbackAsync();
                    return ResultadoOperacion<ProductoDto>.Error(
                        $"Stock insuficiente. Disponible: {inventario.StockDisponible}, Solicitado: {cantidad}",
                        "STOCK_INSUFICIENTE");
                }

                // 6. Crear reserva de inventario
                var resultadoReserva = Domain.Entities.ReservaInventario.Crear(
                    producto.Id,
                    ventaId,
                    cantidad,
                    minutosExpiracion: 15);

                if (resultadoReserva.IsFailure)
                {
                    await _unitOfWork.RollbackAsync();
                    return ResultadoOperacion<ProductoDto>.Error(resultadoReserva.Error);
                }

                // 7. Reservar stock en inventario
                var resultadoReservarStock = inventario.ReservarStock(cantidad);
                if (resultadoReservarStock.IsFailure)
                {
                    await _unitOfWork.RollbackAsync();
                    return ResultadoOperacion<ProductoDto>.Error(resultadoReservarStock.Error);
                }

                // 8. Persistir cambios
                await _unitOfWork.ReservasInventario.AddAsync(resultadoReserva.Value);
                await _unitOfWork.Inventarios.UpdateAsync(inventario);

                await _unitOfWork.CommitAsync();

                // 9. Retornar producto con información actualizada
                var productoDto = producto.ToDto(inventario);
                return ResultadoOperacion<ProductoDto>.Exito(
                    productoDto,
                    $"Producto '{producto.Nombre}' agregado ({cantidad} unidad(es))");
            }
            catch (ProductoNoEncontradoException ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultadoOperacion<ProductoDto>.Error(ex.Message, ex.ErrorCode);
            }
            catch (StockInsuficienteException ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultadoOperacion<ProductoDto>.Error(ex.Message, ex.ErrorCode);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResultadoOperacion<ProductoDto>.Error(
                    $"Error al escanear producto: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}