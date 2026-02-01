using POSSystem.Domain.Common;
using POSSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que controla el stock de un producto.
    /// Maneja stock físico, reservado y disponible.
    /// CRÍTICO: Control de concurrencia para evitar overselling.
    /// </summary>
    public class Inventario : EntityBase
    {
        /// <summary>
        /// Id del producto al que pertenece este inventario.
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// Stock físico real en almacén.
        /// </summary>
        public int StockFisico { get; private set; }

        /// <summary>
        /// Cantidad actualmente reservada (en ventas abiertas).
        /// </summary>
        public int CantidadReservada { get; private set; }

        /// <summary>
        /// Stock mínimo antes de alertar.
        /// </summary>
        public int StockMinimo { get; private set; }

        /// <summary>
        /// Stock máximo permitido.
        /// </summary>
        public int StockMaximo { get; private set; }

        /// <summary>
        /// Última fecha de actualización de stock.
        /// </summary>
        public DateTime UltimaActualizacionStock { get; private set; }

        // Propiedad calculada
        /// <summary>
        /// Stock disponible para venta (Stock Físico - Reservado).
        /// </summary>
        public int StockDisponible => StockFisico - CantidadReservada;

        /// <summary>
        /// Indica si el stock está por debajo del mínimo.
        /// </summary>
        public bool StockBajo => StockFisico <= StockMinimo;

        // Navegación
        public Producto Producto { get; private set; }

        // Constructor privado para EF Core
        private Inventario() { }

        private Inventario(
            Guid productoId,
            int stockInicial,
            int stockMinimo,
            int stockMaximo)
        {
            ProductoId = productoId;
            StockFisico = stockInicial;
            CantidadReservada = 0;
            StockMinimo = stockMinimo;
            StockMaximo = stockMaximo;
            UltimaActualizacionStock = DateTime.UtcNow;
        }

        /// <summary>
        /// Crea un nuevo registro de inventario para un producto.
        /// </summary>
        public static Result<Inventario> Crear(
            Guid productoId,
            int stockInicial = 0,
            int stockMinimo = 10,
            int stockMaximo = 1000)
        {
            if (productoId == Guid.Empty)
                return Result.Failure<Inventario>("El Id del producto es requerido");

            if (stockInicial < 0)
                return Result.Failure<Inventario>("El stock inicial no puede ser negativo");

            if (stockMinimo < 0)
                return Result.Failure<Inventario>("El stock mínimo no puede ser negativo");

            if (stockMaximo <= stockMinimo)
                return Result.Failure<Inventario>("El stock máximo debe ser mayor al stock mínimo");

            if (stockInicial > stockMaximo)
                return Result.Failure<Inventario>("El stock inicial no puede exceder el stock máximo");

            var inventario = new Inventario(productoId, stockInicial, stockMinimo, stockMaximo);

            return Result.Success(inventario);
        }

        // ============================================
        // OPERACIONES DE RESERVA (CRÍTICAS)
        // ============================================

        /// <summary>
        /// Reserva una cantidad de stock temporalmente.
        /// CRÍTICO: Debe ejecutarse dentro de una transacción con lock.
        /// </summary>
        public Result ReservarStock(int cantidad)
        {
            if (cantidad <= 0)
                return Result.Failure("La cantidad a reservar debe ser mayor a cero");

            // Validar stock disponible
            if (StockDisponible < cantidad)
            {
                return Result.Failure(
                    $"Stock insuficiente. Disponible: {StockDisponible}, Solicitado: {cantidad}");
            }

            CantidadReservada += cantidad;
            ActualizarFechaModificacion();

            // Validar invariantes
            ValidarInvariantes();

            return Result.Success();
        }

        /// <summary>
        /// Libera una reserva de stock (cuando se cancela una venta).
        /// </summary>
        public Result LiberarReserva(int cantidad)
        {
            if (cantidad <= 0)
                return Result.Failure("La cantidad a liberar debe ser mayor a cero");

            if (cantidad > CantidadReservada)
                return Result.Failure(
                    $"No se puede liberar {cantidad} unidades. Solo hay {CantidadReservada} reservadas");

            CantidadReservada -= cantidad;
            ActualizarFechaModificacion();

            // Seguridad: nunca menos de cero
            if (CantidadReservada < 0)
                CantidadReservada = 0;

            return Result.Success();
        }

        /// <summary>
        /// Confirma una reserva convirtiéndola en venta real.
        /// CRÍTICO: Descuenta del stock físico y libera la reserva.
        /// </summary>
        public Result ConfirmarReserva(int cantidad)
        {
            if (cantidad <= 0)
                return Result.Failure("La cantidad a confirmar debe ser mayor a cero");

            // Validar que esté reservada
            if (cantidad > CantidadReservada)
                return Result.Failure(
                    $"Reserva inconsistente. Solo hay {CantidadReservada} unidades reservadas");

            // Validar que haya stock físico
            if (cantidad > StockFisico)
                return Result.Failure(
                    $"Stock físico insuficiente. Disponible: {StockFisico}, Solicitado: {cantidad}");

            // Descontar del stock físico
            StockFisico -= cantidad;

            // Liberar la reserva
            CantidadReservada -= cantidad;

            UltimaActualizacionStock = DateTime.UtcNow;
            ActualizarFechaModificacion();

            // Validar invariantes
            ValidarInvariantes();

            return Result.Success();
        }

        // ============================================
        // OPERACIONES DIRECTAS DE STOCK
        // ============================================

        /// <summary>
        /// Incrementa el stock físico (entrada de mercancía).
        /// </summary>
        public Result IncrementarStock(int cantidad)
        {
            if (cantidad <= 0)
                return Result.Failure("La cantidad debe ser mayor a cero");

            var nuevoStock = StockFisico + cantidad;

            if (nuevoStock > StockMaximo)
                return Result.Failure(
                    $"El stock excedería el máximo permitido ({StockMaximo})");

            StockFisico = nuevoStock;
            UltimaActualizacionStock = DateTime.UtcNow;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Decrementa el stock físico (merma, ajuste, etc.).
        /// </summary>
        public Result DecrementarStock(int cantidad)
        {
            if (cantidad <= 0)
                return Result.Failure("La cantidad debe ser mayor a cero");

            var nuevoStock = StockFisico - cantidad;

            if (nuevoStock < 0)
                return Result.Failure("El stock no puede ser negativo");

            // Advertencia si queda stock reservado sin físico
            if (nuevoStock < CantidadReservada)
                return Result.Failure(
                    $"Operación dejaría stock insuficiente para reservas existentes. " +
                    $"Stock resultante: {nuevoStock}, Reservado: {CantidadReservada}");

            StockFisico = nuevoStock;
            UltimaActualizacionStock = DateTime.UtcNow;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Ajusta el stock físico a un valor específico (inventario físico).
        /// </summary>
        public Result AjustarStock(int nuevoStock, string motivo)
        {
            if (nuevoStock < 0)
                return Result.Failure("El stock no puede ser negativo");

            if (nuevoStock < CantidadReservada)
                return Result.Failure(
                    $"El stock no puede ser menor a la cantidad reservada ({CantidadReservada})");

            if (string.IsNullOrWhiteSpace(motivo))
                return Result.Failure("Debe proporcionar un motivo para el ajuste");

            StockFisico = nuevoStock;
            UltimaActualizacionStock = DateTime.UtcNow;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Actualiza los límites de stock.
        /// </summary>
        public Result ActualizarLimites(int stockMinimo, int stockMaximo)
        {
            if (stockMinimo < 0)
                return Result.Failure("El stock mínimo no puede ser negativo");

            if (stockMaximo <= stockMinimo)
                return Result.Failure("El stock máximo debe ser mayor al stock mínimo");

            StockMinimo = stockMinimo;
            StockMaximo = stockMaximo;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        // ============================================
        // VALIDACIÓN DE INVARIANTES
        // ============================================

        private void ValidarInvariantes()
        {
            // INVARIANTE 1: Stock físico nunca negativo
            if (StockFisico < 0)
                throw new InvarianteVioladaException("Stock físico negativo");

            // INVARIANTE 2: Cantidad reservada nunca negativa
            if (CantidadReservada < 0)
                throw new InvarianteVioladaException("Cantidad reservada negativa");

            // INVARIANTE 3: Stock disponible nunca negativo
            if (StockDisponible < 0)
                throw new InvarianteVioladaException(
                    $"Stock disponible negativo. Físico: {StockFisico}, Reservado: {CantidadReservada}");

            // INVARIANTE 4: Reservas no pueden exceder stock físico
            if (CantidadReservada > StockFisico)
                throw new InvarianteVioladaException(
                    $"Reservas ({CantidadReservada}) exceden stock físico ({StockFisico})");
        }
    }
}
