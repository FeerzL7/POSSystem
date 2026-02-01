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
    /// Entidad que representa un movimiento de caja (auditoría).
    /// INMUTABLE una vez creada.
    /// No puede existir independientemente de una Caja.
    /// </summary>
    public class MovimientoCaja : EntityBase
    {
        /// <summary>
        /// Id de la caja a la que pertenece.
        /// </summary>
        public Guid CajaId { get; private set; }

        /// <summary>
        /// Tipo de movimiento.
        /// </summary>
        public TipoMovimientoCaja TipoMovimiento { get; private set; }

        /// <summary>
        /// Monto del movimiento (positivo = entrada, negativo = salida).
        /// </summary>
        public decimal Monto { get; private set; }

        /// <summary>
        /// Concepto o descripción del movimiento.
        /// </summary>
        public string Concepto { get; private set; }

        /// <summary>
        /// Referencia externa (folio de venta, número de autorización, etc.).
        /// </summary>
        public string Referencia { get; private set; }

        /// <summary>
        /// Id del usuario que realizó el movimiento.
        /// </summary>
        public Guid UsuarioId { get; private set; }

        /// <summary>
        /// Fecha y hora del movimiento.
        /// </summary>
        public DateTime FechaMovimiento { get; private set; }

        // Navegación
        public Caja Caja { get; private set; }

        // Constructor privado para EF Core
        private MovimientoCaja() { }

        private MovimientoCaja(
            Guid cajaId,
            TipoMovimientoCaja tipoMovimiento,
            decimal monto,
            string concepto,
            Guid usuarioId,
            string referencia)
        {
            CajaId = cajaId;
            TipoMovimiento = tipoMovimiento;
            Monto = monto;
            Concepto = concepto;
            UsuarioId = usuarioId;
            Referencia = referencia ?? string.Empty;
            FechaMovimiento = DateTime.UtcNow;
        }

        /// <summary>
        /// Crea un nuevo movimiento de caja.
        /// </summary>
        public static Result<MovimientoCaja> Crear(
            Guid cajaId,
            TipoMovimientoCaja tipoMovimiento,
            decimal monto,
            string concepto,
            Guid usuarioId,
            string referencia = null)
        {
            if (cajaId == Guid.Empty)
                return Result.Failure<MovimientoCaja>("El Id de la caja es requerido");

            if (string.IsNullOrWhiteSpace(concepto))
                return Result.Failure<MovimientoCaja>("El concepto es requerido");

            if (concepto.Length > 500)
                return Result.Failure<MovimientoCaja>("El concepto no puede exceder 500 caracteres");

            if (usuarioId == Guid.Empty)
                return Result.Failure<MovimientoCaja>("El Id del usuario es requerido");

            // Validar que movimientos de salida sean negativos
            if ((tipoMovimiento == TipoMovimientoCaja.CancelacionVenta ||
                 tipoMovimiento == TipoMovimientoCaja.Retiro) && monto > 0)
            {
                monto = -Math.Abs(monto); // Forzar negativo
            }

            // Validar que movimientos de entrada sean positivos
            if ((tipoMovimiento == TipoMovimientoCaja.Venta ||
                 tipoMovimiento == TipoMovimientoCaja.Deposito ||
                 tipoMovimiento == TipoMovimientoCaja.Apertura) && monto < 0)
            {
                return Result.Failure<MovimientoCaja>(
                    $"El monto para {tipoMovimiento} debe ser positivo");
            }

            var movimiento = new MovimientoCaja(
                cajaId,
                tipoMovimiento,
                monto,
                concepto.Trim(),
                usuarioId,
                referencia?.Trim());

            return Result.Success(movimiento);
        }
    }
}
