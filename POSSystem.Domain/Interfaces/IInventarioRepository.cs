using POSSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el repositorio de inventario.
    /// </summary>
    public interface IInventarioRepository
    {
        /// <summary>
        /// Obtiene un inventario por su Id.
        /// </summary>
        Task<Inventario> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene el inventario de un producto específico.
        /// </summary>
        Task<Inventario> GetByProductoIdAsync(Guid productoId);

        /// <summary>
        /// Obtiene múltiples inventarios por Ids de productos.
        /// </summary>
        Task<IEnumerable<Inventario>> GetByProductoIdsAsync(IEnumerable<Guid> productoIds);

        /// <summary>
        /// Obtiene todos los inventarios.
        /// </summary>
        Task<IEnumerable<Inventario>> GetAllAsync();

        /// <summary>
        /// Obtiene productos con stock bajo.
        /// </summary>
        Task<IEnumerable<Inventario>> GetProductosStockBajoAsync();

        /// <summary>
        /// Obtiene productos sin stock.
        /// </summary>
        Task<IEnumerable<Inventario>> GetProductosSinStockAsync();

        /// <summary>
        /// Agrega un nuevo registro de inventario.
        /// </summary>
        Task AddAsync(Inventario inventario);

        /// <summary>
        /// Actualiza un inventario existente.
        /// </summary>
        Task UpdateAsync(Inventario inventario);

        /// <summary>
        /// Obtiene el inventario con bloqueo para actualización (FOR UPDATE).
        /// CRÍTICO: Previene race conditions.
        /// </summary>
        Task<Inventario> GetByProductoIdWithLockAsync(Guid productoId);

        /// <summary>
        /// Verifica si existe un inventario para un producto.
        /// </summary>
        Task<bool> ExisteInventarioParaProductoAsync(Guid productoId);
    }
}
