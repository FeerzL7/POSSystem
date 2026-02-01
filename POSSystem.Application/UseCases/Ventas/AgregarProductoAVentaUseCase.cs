using System;
using System.Threading.Tasks;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Ventas
{
    /// <summary>
    /// Caso de uso: Agregar un producto a una venta existente.
    /// Asume que ya se escaneó y validó el producto.
    /// </summary>
    public class AgregarProductoAVentaUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AgregarProductoAVentaUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ResultadoOperacion<VentaDto>> ExecuteAsync(
            Guid ventaId,
            Guid productoId,
            int cantidad = 1)
        {
            try
            {
                // 1. Cargar venta (en memoria, no de BD aún)
                // NOTA: En implementación real, la venta estaría en memoria en el ViewModel
                // Este método es para cuando necesitemos persistir temporalmente

                // 2. Obtener producto
                var producto = await _unitOfWork.Productos.GetByIdAsync(productoId);
                if (producto == null)
                {
                    return ResultadoOperacion<VentaDto>.Error(
                        "Producto no encontrado",
                        "PRODUCTO_NO_ENCONTRADO");
                }

                // 3. La lógica real está en EscanearProductoUseCase
                // Este método es un wrapper simplificado

                return ResultadoOperacion<VentaDto>.Error(
                    "Use EscanearProductoUseCase para agregar productos",
                    "METODO_DEPRECADO");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<VentaDto>.Error(
                    $"Error al agregar producto: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}