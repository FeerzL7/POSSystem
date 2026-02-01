using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Enums
{
    /// <summary>
    /// Tipos de movimientos de inventario para auditoría.
    /// Registro inmutable de cambios en stock.
    /// </summary>
    public enum TipoMovimientoInventario
    {
        /// <summary>
        /// Entrada de mercancía (compra a proveedor).
        /// Incrementa stock.
        /// </summary>
        Entrada = 0,

        /// <summary>
        /// Salida por venta.
        /// Decrementa stock.
        /// </summary>
        Venta = 1,

        /// <summary>
        /// Devolución de cliente.
        /// Incrementa stock.
        /// </summary>
        Devolucion = 2,

        /// <summary>
        /// Ajuste de inventario (físico vs sistema).
        /// Puede incrementar o decrementar.
        /// </summary>
        Ajuste = 3,

        /// <summary>
        /// Merma o producto dañado.
        /// Decrementa stock.
        /// </summary>
        Merma = 4,

        /// <summary>
        /// Traspaso entre sucursales (salida).
        /// Decrementa stock.
        /// </summary>
        TraspasoSalida = 5,

        /// <summary>
        /// Traspaso entre sucursales (entrada).
        /// Incrementa stock.
        /// </summary>
        TraspasoEntrada = 6
    }
}
