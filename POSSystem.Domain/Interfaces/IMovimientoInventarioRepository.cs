using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el repositorio de movimientos de inventario (auditoría).
    /// </summary>
    public interface IMovimientoInventarioRepository
    {
        /// <summary>
        /// Obtiene un movimiento por su Id.
        /// </summary>
        Task<MovimientoInventario> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene movimientos de un producto específico.
        /// </summary>
        Task<IEnumerable<MovimientoInventario>> GetPorProductoIdAsync(
            Guid productoId,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene movimientos por tipo.
        /// </summary>
        Task<IEnumerable<MovimientoInventario>> GetPorTipoAsync(
            TipoMovimientoInventario tipo,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null);

        /// <summary>
        /// Obtiene movimientos por referencia (folio de venta, orden de compra, etc.).
        /// </summary>
        Task<IEnumerable<MovimientoInventario>> GetPorReferenciaAsync(string referencia);

        /// <summary>
        /// Obtiene movimientos en un rango de fechas.
        /// </summary>
        Task<IEnumerable<MovimientoInventario>> GetPorRangoFechasAsync(
            DateTime fechaInicio,
            DateTime fechaFin);

        /// <summary>
        /// Agrega un nuevo movimiento.
        /// </summary>
        Task AddAsync(MovimientoInventario movimiento);

        /// <summary>
        /// Obtiene el historial completo de un producto.
        /// </summary>
        Task<IEnumerable<MovimientoInventario>> GetHistorialProductoAsync(Guid productoId);
    }
}
