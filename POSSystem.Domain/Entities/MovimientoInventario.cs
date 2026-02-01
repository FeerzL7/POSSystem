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
    /// Entidad que representa un movimiento de inventario (auditoría).
    /// INMUTABLE una vez creada.
    /// Registra todos los cambios en stock para trazabilidad.
    /// </summary>
    public class MovimientoInventario : EntityBase
    {
        /// <summary>
        /// Id del producto afectado.
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// Tipo de movimiento.
        /// </summary>
        public TipoMovimientoInventario TipoMovimiento { get; private set; }

        /// <summary>
        /// Cantidad del movimiento (positivo = entrada, negativo = salida).
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Stock anterior al movimiento.
        /// </summary>
        public int StockAnterior { get; private set; }

        /// <summary>
        /// Stock posterior al movimiento.
        /// </summary>
        public int StockPosterior { get; private set; }

        /// <summary>
        /// Concepto o descripción del movimiento.
        /// </summary>
        public string Concepto { get; private set; }

        /// <summary>
        /// Referencia externa (folio de venta, orden de compra, etc.).
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
        public Producto Producto { get; private set; }

        // Constructor privado para EF Core
        private MovimientoInventario() { }

        private MovimientoInventario(
            Guid productoId,
            TipoMovimientoInventario tipoMovimiento,
            int cantidad,
            int stockAnterior,
            int stockPosterior,
            string concepto,
            Guid usuarioId,
            string referencia)
        {
            ProductoId = productoId;
            TipoMovimiento = tipoMovimiento;
            Cantidad = cantidad;
            StockAnterior = stockAnterior;
            StockPosterior = stockPosterior;
            Concepto = concepto;
            UsuarioId = usuarioId;
            Referencia = referencia ?? string.Empty;
            FechaMovimiento = DateTime.UtcNow;
        }

        /// <summary>
        /// Crea un nuevo movimiento de inventario.
        /// </summary>
        public static Result<MovimientoInventario> Crear(
            Guid productoId,
            TipoMovimientoInventario tipoMovimiento,
            int cantidad,
            int stockAnterior,
            int stockPosterior,
            string concepto,
            Guid usuarioId,
            string referencia = null)
        {
            if (productoId == Guid.Empty)
                return Result.Failure<MovimientoInventario>("El Id del producto es requerido");

            if (cantidad == 0)
                return Result.Failure<MovimientoInventario>("La cantidad no puede ser cero");

            if (stockAnterior < 0 || stockPosterior < 0)
                return Result.Failure<MovimientoInventario>("Los stocks no pueden ser negativos");

            if (string.IsNullOrWhiteSpace(concepto))
                return Result.Failure<MovimientoInventario>("El concepto es requerido");

            if (concepto.Length > 500)
                return Result.Failure<MovimientoInventario>("El concepto no puede exceder 500 caracteres");

            if (usuarioId == Guid.Empty)
                return Result.Failure<MovimientoInventario>("El Id del usuario es requerido");

            // Validar consistencia de stocks
            var diferencia = stockPosterior - stockAnterior;
            if (diferencia != cantidad)
                return Result.Failure<MovimientoInventario>(
                    $"Inconsistencia en movimiento. Diferencia: {diferencia}, Cantidad: {cantidad}");

            // Validar que movimientos de salida sean negativos
            if ((tipoMovimiento == TipoMovimientoInventario.Venta ||
                 tipoMovimiento == TipoMovimientoInventario.Merma ||
                 tipoMovimiento == TipoMovimientoInventario.TraspasoSalida) && cantidad > 0)
            {
                cantidad = -Math.Abs(cantidad);
            }

            // Validar que movimientos de entrada sean positivos
            if ((tipoMovimiento == TipoMovimientoInventario.Entrada ||
                 tipoMovimiento == TipoMovimientoInventario.Devolucion ||
                 tipoMovimiento == TipoMovimientoInventario.TraspasoEntrada) && cantidad < 0)
            {
                return Result.Failure<MovimientoInventario>(
                    $"El movimiento {tipoMovimiento} debe tener cantidad positiva");
            }

            var movimiento = new MovimientoInventario(
                productoId,
                tipoMovimiento,
                cantidad,
                stockAnterior,
                stockPosterior,
                concepto.Trim(),
                usuarioId,
                referencia?.Trim());

            return Result.Success(movimiento);
        }
    }
}
