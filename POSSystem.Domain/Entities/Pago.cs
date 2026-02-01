using POSSystem.Domain.Common;
using POSSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un pago realizado en una venta.
    /// No puede existir independientemente de una Venta.
    /// </summary>
    public class Pago : EntityBase
    {
        /// <summary>
        /// Id de la venta a la que pertenece.
        /// </summary>
        public Guid VentaId { get; private set; }

        /// <summary>
        /// Monto pagado.
        /// </summary>
        public decimal Monto { get; private set; }

        /// <summary>
        /// Tipo de pago (Efectivo, Tarjeta, etc.).
        /// </summary>
        public TipoPago TipoPago { get; private set; }

        /// <summary>
        /// Fecha y hora del pago.
        /// </summary>
        public DateTime FechaPago { get; private set; }

        /// <summary>
        /// Referencia o número de autorización (para tarjetas/transferencias).
        /// </summary>
        public string Referencia { get; private set; }

        /// <summary>
        /// Cambio devuelto (solo para efectivo).
        /// </summary>
        public decimal Cambio { get; private set; }

        // Constructor privado para EF Core
        private Pago() { }

        private Pago(
            Guid ventaId,
            decimal monto,
            TipoPago tipoPago,
            decimal cambio = 0,
            string referencia = null)
        {
            VentaId = ventaId;
            Monto = monto;
            TipoPago = tipoPago;
            FechaPago = DateTime.UtcNow;
            Cambio = cambio;
            Referencia = referencia ?? string.Empty;
        }

        /// <summary>
        /// Crea un nuevo pago.
        /// </summary>
        public static Result<Pago> Crear(
            Guid ventaId,
            decimal monto,
            TipoPago tipoPago,
            decimal totalVenta,
            string referencia = null)
        {
            // Validaciones
            if (ventaId == Guid.Empty)
                return Result.Failure<Pago>("El Id de la venta es requerido");

            if (monto <= 0)
                return Result.Failure<Pago>("El monto del pago debe ser mayor a cero");

            if (totalVenta <= 0)
                return Result.Failure<Pago>("El total de la venta debe ser mayor a cero");

            // Validar monto suficiente
            if (monto < totalVenta)
                return Result.Failure<Pago>($"Pago insuficiente. Requerido: {totalVenta:C}, Pagado: {monto:C}");

            // Calcular cambio (solo para efectivo)
            decimal cambio = 0;
            if (tipoPago == TipoPago.Efectivo)
            {
                cambio = monto - totalVenta;
            }
            else
            {
                // Para otros métodos de pago, el monto debe ser exacto
                if (monto != totalVenta)
                    return Result.Failure<Pago>($"El pago con {tipoPago} debe ser por el monto exacto");
            }

            // Validar referencia para pagos electrónicos
            if ((tipoPago == TipoPago.TarjetaDebito ||
                 tipoPago == TipoPago.TarjetaCredito ||
                 tipoPago == TipoPago.Transferencia)
                && string.IsNullOrWhiteSpace(referencia))
            {
                return Result.Failure<Pago>("Se requiere referencia para pagos electrónicos");
            }

            var pago = new Pago(ventaId, monto, tipoPago, cambio, referencia);

            return Result.Success(pago);
        }
    }
}
