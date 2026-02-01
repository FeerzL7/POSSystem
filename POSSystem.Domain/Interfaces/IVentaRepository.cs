using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using POSSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el repositorio de ventas.
    /// </summary>
    public interface IVentaRepository
    {
        /// <summary>
        /// Obtiene una venta por su Id (incluyendo detalles y pagos).
        /// </summary>
        Task<Venta> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene una venta por su folio.
        /// </summary>
        Task<Venta> GetByFolioAsync(Folio folio);

        /// <summary>
        /// Obtiene una venta por su folio (string).
        /// </summary>
        Task<Venta> GetByFolioAsync(string folio);

        /// <summary>
        /// Obtiene ventas por rango de fechas.
        /// </summary>
        Task<IEnumerable<Venta>> GetPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Obtiene ventas por estado.
        /// </summary>
        Task<IEnumerable<Venta>> GetPorEstadoAsync(EstadoVenta estado);

        /// <summary>
        /// Obtiene ventas abiertas (en proceso).
        /// </summary>
        Task<IEnumerable<Venta>> GetVentasAbiertasAsync();

        /// <summary>
        /// Obtiene ventas de un usuario específico.
        /// </summary>
        Task<IEnumerable<Venta>> GetPorUsuarioAsync(Guid usuarioId, DateTime? fecha = null);

        /// <summary>
        /// Agrega una nueva venta.
        /// </summary>
        Task AddAsync(Venta venta);

        /// <summary>
        /// Actualiza una venta existente.
        /// </summary>
        Task UpdateAsync(Venta venta);

        /// <summary>
        /// Obtiene el total de ventas del día.
        /// </summary>
        Task<decimal> GetTotalVentasDelDiaAsync(DateTime? fecha = null);

        /// <summary>
        /// Obtiene el conteo de ventas por estado en un rango de fechas.
        /// </summary>
        Task<Dictionary<EstadoVenta, int>> GetConteoVentasPorEstadoAsync(
            DateTime fechaInicio, DateTime fechaFin);

        /// <summary>
        /// Verifica si existe un folio.
        /// </summary>
        Task<bool> ExisteFolioAsync(string folio);

        /// <summary>
        /// Obtiene ventas abandonadas (abiertas por más de X minutos).
        /// </summary>
        Task<IEnumerable<Venta>> GetVentasAbandonadasAsync(int minutosInactividad);
    }
}
