using POSSystem.Domain.Entities;
using POSSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el repositorio de productos.
    /// </summary>
    public interface IProductoRepository
    {
        /// <summary>
        /// Obtiene un producto por su Id.
        /// </summary>
        Task<Producto> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene un producto por su código de barras.
        /// </summary>
        Task<Producto> GetByCodigoBarrasAsync(CodigoBarras codigoBarras);

        /// <summary>
        /// Obtiene un producto por su código de barras (string).
        /// </summary>
        Task<Producto> GetByCodigoBarrasAsync(string codigoBarras);

        /// <summary>
        /// Obtiene todos los productos activos.
        /// </summary>
        Task<IEnumerable<Producto>> GetAllActivosAsync();

        /// <summary>
        /// Obtiene todos los productos.
        /// </summary>
        Task<IEnumerable<Producto>> GetAllAsync();

        /// <summary>
        /// Busca productos por nombre.
        /// </summary>
        Task<IEnumerable<Producto>> BuscarPorNombreAsync(string nombre);

        /// <summary>
        /// Busca productos por categoría.
        /// </summary>
        Task<IEnumerable<Producto>> GetPorCategoriaAsync(string categoria);

        /// <summary>
        /// Agrega un nuevo producto.
        /// </summary>
        Task AddAsync(Producto producto);

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        Task UpdateAsync(Producto producto);

        /// <summary>
        /// Elimina un producto (soft delete).
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Verifica si existe un producto con el código de barras especificado.
        /// </summary>
        Task<bool> ExisteCodigoBarrasAsync(string codigoBarras);

        /// <summary>
        /// Obtiene múltiples productos por sus Ids.
        /// </summary>
        Task<IEnumerable<Producto>> GetByIdsAsync(IEnumerable<Guid> ids);
    }
}
