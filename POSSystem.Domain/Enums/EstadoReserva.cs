using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Enums
{
    /// <summary>
    /// Estados posibles de una reserva de inventario.
    /// Controla el ciclo de vida de reservas temporales de stock.
    /// </summary>
    public enum EstadoReserva
    {
        /// <summary>
        /// Reserva activa, bloqueando stock.
        /// </summary>
        Activa = 0,

        /// <summary>
        /// Reserva confirmada (convertida en venta).
        /// Estado final.
        /// </summary>
        Confirmada = 1,

        /// <summary>
        /// Reserva cancelada manualmente.
        /// Estado final.
        /// </summary>
        Cancelada = 2,

        /// <summary>
        /// Reserva expirada por timeout.
        /// Estado final.
        /// </summary>
        Expirada = 3
    }
}
