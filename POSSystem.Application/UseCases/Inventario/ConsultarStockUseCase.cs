using System;
using System.Threading.Tasks;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Inventario
{
    /// <summary>
    /// Caso de uso: Consultar stock disponible de un producto.
    /// </summary>
    public class ConsultarStockUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConsultarStockUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ResultadoOperacion<ProductoDto>> ExecuteAsync(string codigoBarras)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByCodigoBarrasAsync(codigoBarras);
                if (producto == null)
                {
                    return ResultadoOperacion<ProductoDto>.Error(
                        $"Producto no encontrado: {codigoBarras}",
                        "PRODUCTO_NO_ENCONTRADO");
                }

                var inventario = await _unitOfWork.Inventarios.GetByProductoIdAsync(producto.Id);

                var productoDto = producto.ToDto(inventario);
                return ResultadoOperacion<ProductoDto>.Exito(
                    productoDto,
                    $"Stock disponible: {inventario?.StockDisponible ?? 0}");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<ProductoDto>.Error(
                    $"Error al consultar stock: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}