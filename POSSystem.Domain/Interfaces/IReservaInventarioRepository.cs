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
    /// Contrato para el repositorio de reservas de inventario.
    /// </summary>
    public interface IReservaInventarioRepository
    {
        /// <summary>
        /// Obtiene una reserva por su Id.
        /// </summary>
        Task<ReservaInventario> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene todas las reservas de una venta.
        /// </summary>
        Task<IEnumerable<ReservaInventario>> GetPorVentaIdAsync(Guid ventaId);

        /// <summary>
        /// Obtiene todas las reservas activas de un producto.
        /// </summary>
        Task<IEnumerable<ReservaInventario>> GetReservasActivasPorProductoAsync(Guid productoId);

        /// <summary>
        /// Obtiene todas las reservas activas.
        /// </summary>
        Task<IEnumerable<ReservaInventario>> GetReservasActivasAsync();

        /// <summary>
        /// Obtiene reservas expiradas que aún no han sido procesadas.
        /// </summary>
        Task<IEnumerable<ReservaInventario>> GetReservasExpiradasAsync();

        /// <summary>
        /// Obtiene reservas por estado.
        /// </summary>
        Task<IEnumerable<ReservaInventario>> GetPorEstadoAsync(EstadoReserva estado);

        /// <summary>
        /// Agrega una nueva reserva.
        /// </summary>
        Task AddAsync(ReservaInventario reserva);

        /// <summary>
        /// Actualiza una reserva existente.
        /// </summary>
        Task UpdateAsync(ReservaInventario reserva);

        /// <summary>
        /// Obtiene la cantidad total reservada de un producto.
        /// </summary>
        Task<int> GetCantidadReservadaPorProductoAsync(Guid productoId);

        /// <summary>
        /// Elimina reservas antiguas (limpieza de datos históricos).
        /// </summary>
        Task EliminarReservasAntiguasAsync(DateTime fechaLimite);
    }
}
