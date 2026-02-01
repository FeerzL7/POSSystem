using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Enums
{
    /// <summary>
    /// Tipos de movimientos en caja para auditoría.
    /// Registro inmutable de flujos de efectivo.
    /// </summary>
    public enum TipoMovimientoCaja
    {
        /// <summary>
        /// Apertura de caja (fondo inicial).
        /// </summary>
        Apertura = 0,

        /// <summary>
        /// Ingreso por venta.
        /// Incrementa el efectivo en caja.
        /// </summary>
        Venta = 1,

        /// <summary>
        /// Egreso por cancelación de venta.
        /// Decrementa el efectivo en caja.
        /// </summary>
        CancelacionVenta = 2,

        /// <summary>
        /// Retiro de efectivo (corte parcial).
        /// Decrementa el efectivo en caja.
        /// </summary>
        Retiro = 3,

        /// <summary>
        /// Ingreso por deposito manual.
        /// Incrementa el efectivo en caja.
        /// </summary>
        Deposito = 4,

        /// <summary>
        /// Cierre de caja (corte final).
        /// </summary>
        Cierre = 5,

        /// <summary>
        /// Ajuste por faltante o sobrante.
        /// Puede ser positivo o negativo.
        /// </summary>
        Ajuste = 6
    }
}
