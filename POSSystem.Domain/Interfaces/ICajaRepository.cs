using POSSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el repositorio de cajas.
    /// </summary>
    public interface ICajaRepository
    {
        /// <summary>
        /// Obtiene una caja por su Id (incluyendo movimientos).
        /// </summary>
        Task<Caja> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene una caja por su número.
        /// </summary>
        Task<Caja> GetByNumeroAsync(int numeroCaja);

        /// <summary>
        /// Obtiene la caja actualmente abierta (si existe).
        /// </summary>
        Task<Caja> GetCajaAbiertaAsync();

        /// <summary>
        /// Obtiene todas las cajas.
        /// </summary>
        Task<IEnumerable<Caja>> GetAllAsync();

        /// <summary>
        /// Obtiene cajas por estado (abiertas/cerradas).
        /// </summary>
        Task<IEnumerable<Caja>> GetPorEstadoAsync(bool estaAbierta);

        /// <summary>
        /// Obtiene el historial de cierres de una caja.
        /// </summary>
        Task<IEnumerable<Caja>> GetHistorialCierresAsync(
            int numeroCaja,
            DateTime fechaInicio,
            DateTime fechaFin);

        /// <summary>
        /// Agrega una nueva caja.
        /// </summary>
        Task AddAsync(Caja caja);

        /// <summary>
        /// Actualiza una caja existente.
        /// </summary>
        Task UpdateAsync(Caja caja);

        /// <summary>
        /// Verifica si hay una caja abierta.
        /// </summary>
        Task<bool> ExisteCajaAbiertaAsync();

        /// <summary>
        /// Obtiene el total de ventas de una caja en un rango de fechas.
        /// </summary>
        Task<decimal> GetTotalVentasCajaAsync(Guid cajaId, DateTime fechaInicio, DateTime fechaFin);
    }
}
