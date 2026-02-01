using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Enums
{
    /// <summary>
    /// Métodos de pago aceptados en el sistema.
    /// </summary>
    public enum TipoPago
    {
        /// <summary>
        /// Pago en efectivo.
        /// Genera cambio.
        /// </summary>
        Efectivo = 0,

        /// <summary>
        /// Pago con tarjeta de débito.
        /// Sin cambio.
        /// </summary>
        TarjetaDebito = 1,

        /// <summary>
        /// Pago con tarjeta de crédito.
        /// Sin cambio.
        /// </summary>
        TarjetaCredito = 2,

        /// <summary>
        /// Transferencia electrónica.
        /// Sin cambio.
        /// </summary>
        Transferencia = 3,

        /// <summary>
        /// Pago mixto (múltiples métodos).
        /// Futuro: separar en pagos individuales.
        /// </summary>
        Mixto = 4
    }
}
