using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Enums
{
    /// <summary>
    /// Estados posibles de una venta en el sistema.
    /// Representa la máquina de estados del ciclo de vida de una venta.
    /// </summary>
    public enum EstadoVenta
    {
        /// <summary>
        /// Venta recién creada, sin productos agregados.
        /// </summary>
        Nueva = 0,

        /// <summary>
        /// Venta con productos, esperando pago.
        /// Estado operativo principal.
        /// </summary>
        Abierta = 1,

        /// <summary>
        /// Venta pagada y finalizada exitosamente.
        /// Estado final inmutable.
        /// </summary>
        Pagada = 2,

        /// <summary>
        /// Venta cancelada (antes o después del pago).
        /// Estado final inmutable.
        /// </summary>
        Cancelada = 3
    }
}
