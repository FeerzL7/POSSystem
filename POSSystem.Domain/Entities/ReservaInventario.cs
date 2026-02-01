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
    /// Entidad que representa una reserva temporal de inventario.
    /// Asociada a una venta en proceso.
    /// Expira automáticamente si no se confirma en el tiempo establecido.
    /// </summary>
    public class ReservaInventario : EntityBase
    {
        /// <summary>
        /// Id del producto reservado.
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// Id de la venta asociada.
        /// </summary>
        public Guid VentaId { get; private set; }

        /// <summary>
        /// Cantidad reservada.
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Fecha de expiración de la reserva.
        /// </summary>
        public DateTime FechaExpiracion { get; private set; }

        /// <summary>
        /// Estado actual de la reserva.
        /// </summary>
        public EstadoReserva Estado { get; private set; }

        /// <summary>
        /// Fecha de confirmación (si aplica).
        /// </summary>
        public DateTime? FechaConfirmacion { get; private set; }

        /// <summary>
        /// Fecha de cancelación (si aplica).
        /// </summary>
        public DateTime? FechaCancelacion { get; private set; }

        /// <summary>
        /// Motivo de cancelación (si aplica).
        /// </summary>
        public string MotivoCancelacion { get; private set; }

        // Navegación
        public Producto Producto { get; private set; }
        public Venta Venta { get; private set; }

        // Constructor privado para EF Core
        private ReservaInventario() { }

        private ReservaInventario(
            Guid productoId,
            Guid ventaId,
            int cantidad,
            int minutosExpiracion)
        {
            ProductoId = productoId;
            VentaId = ventaId;
            Cantidad = cantidad;
            FechaExpiracion = DateTime.UtcNow.AddMinutes(minutosExpiracion);
            Estado = EstadoReserva.Activa;
        }

        /// <summary>
        /// Crea una nueva reserva de inventario.
        /// </summary>
        public static Result<ReservaInventario> Crear(
            Guid productoId,
            Guid ventaId,
            int cantidad,
            int minutosExpiracion = 15)
        {
            if (productoId == Guid.Empty)
                return Result.Failure<ReservaInventario>("El Id del producto es requerido");

            if (ventaId == Guid.Empty)
                return Result.Failure<ReservaInventario>("El Id de la venta es requerido");

            if (cantidad <= 0)
                return Result.Failure<ReservaInventario>("La cantidad debe ser mayor a cero");

            if (minutosExpiracion < 1 || minutosExpiracion > 60)
                return Result.Failure<ReservaInventario>(
                    "Los minutos de expiración deben estar entre 1 y 60");

            var reserva = new ReservaInventario(productoId, ventaId, cantidad, minutosExpiracion);

            return Result.Success(reserva);
        }

        /// <summary>
        /// Verifica si la reserva ha expirado.
        /// </summary>
        public bool EstaExpirada()
        {
            return DateTime.UtcNow > FechaExpiracion && Estado == EstadoReserva.Activa;
        }

        /// <summary>
        /// Confirma la reserva (venta finalizada).
        /// TRANSICIÓN: Activa → Confirmada
        /// </summary>
        public Result Confirmar()
        {
            if (Estado != EstadoReserva.Activa)
                return Result.Failure($"No se puede confirmar una reserva en estado {Estado}");

            if (EstaExpirada())
                return Result.Failure("No se puede confirmar una reserva expirada");

            Estado = EstadoReserva.Confirmada;
            FechaConfirmacion = DateTime.UtcNow;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Cancela la reserva manualmente.
        /// TRANSICIÓN: Activa → Cancelada
        /// </summary>
        public Result Cancelar(string motivo)
        {
            if (Estado != EstadoReserva.Activa)
                return Result.Failure($"No se puede cancelar una reserva en estado {Estado}");

            if (string.IsNullOrWhiteSpace(motivo))
                return Result.Failure("Debe proporcionar un motivo de cancelación");

            Estado = EstadoReserva.Cancelada;
            FechaCancelacion = DateTime.UtcNow;
            MotivoCancelacion = motivo.Trim();
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Expira la reserva automáticamente.
        /// TRANSICIÓN: Activa → Expirada
        /// </summary>
        public Result Expirar()
        {
            if (Estado != EstadoReserva.Activa)
                return Result.Failure($"No se puede expirar una reserva en estado {Estado}");

            if (!EstaExpirada())
                return Result.Failure("La reserva aún no ha expirado");

            Estado = EstadoReserva.Expirada;
            FechaCancelacion = DateTime.UtcNow;
            MotivoCancelacion = "Reserva expirada automáticamente por timeout";
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Extiende el tiempo de expiración de la reserva.
        /// </summary>
        public Result ExtenderExpiracion(int minutosAdicionales)
        {
            if (Estado != EstadoReserva.Activa)
                return Result.Failure("Solo se pueden extender reservas activas");

            if (minutosAdicionales <= 0 || minutosAdicionales > 60)
                return Result.Failure("Los minutos adicionales deben estar entre 1 y 60");

            FechaExpiracion = FechaExpiracion.AddMinutes(minutosAdicionales);
            ActualizarFechaModificacion();

            return Result.Success();
        }
    }
}
