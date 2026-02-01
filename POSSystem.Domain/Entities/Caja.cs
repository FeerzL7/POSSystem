using POSSystem.Domain.Common;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa una caja de punto de venta.
    /// Controla el efectivo y los movimientos durante un turno.
    /// CRÍTICO: Mantiene consistencia del efectivo.
    /// </summary>
    public class Caja : EntityBase
    {
        private readonly List<MovimientoCaja> _movimientos = new();

        /// <summary>
        /// Número de caja (identificador físico).
        /// </summary>
        public int NumeroCaja { get; private set; }

        /// <summary>
        /// Nombre de la caja.
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Indica si la caja está abierta.
        /// </summary>
        public bool EstaAbierta { get; private set; }

        /// <summary>
        /// Fondo inicial con el que se abrió la caja.
        /// </summary>
        public decimal FondoInicial { get; private set; }

        /// <summary>
        /// Saldo actual en efectivo.
        /// </summary>
        public decimal SaldoActual { get; private set; }

        /// <summary>
        /// Fecha y hora de apertura.
        /// </summary>
        public DateTime? FechaApertura { get; private set; }

        /// <summary>
        /// Fecha y hora de cierre.
        /// </summary>
        public DateTime? FechaCierre { get; private set; }

        /// <summary>
        /// Id del usuario que abrió la caja.
        /// </summary>
        public Guid? UsuarioAperturaId { get; private set; }

        /// <summary>
        /// Id del usuario que cerró la caja.
        /// </summary>
        public Guid? UsuarioCierreId { get; private set; }

        /// <summary>
        /// Saldo final al cerrar (puede diferir del calculado).
        /// </summary>
        public decimal? SaldoFinalDeclarado { get; private set; }

        /// <summary>
        /// Diferencia entre saldo calculado y declarado.
        /// </summary>
        public decimal? Diferencia { get; private set; }

        /// <summary>
        /// Observaciones del cierre.
        /// </summary>
        public string ObservacionesCierre { get; private set; }

        // Exposición de movimientos como solo lectura
        public IReadOnlyCollection<MovimientoCaja> Movimientos => _movimientos.AsReadOnly();

        // Propiedades calculadas
        /// <summary>
        /// Total de ventas registradas.
        /// </summary>
        public decimal TotalVentas => _movimientos
            .Where(m => m.TipoMovimiento == TipoMovimientoCaja.Venta)
            .Sum(m => m.Monto);

        /// <summary>
        /// Total de cancelaciones.
        /// </summary>
        public decimal TotalCancelaciones => _movimientos
            .Where(m => m.TipoMovimiento == TipoMovimientoCaja.CancelacionVenta)
            .Sum(m => Math.Abs(m.Monto));

        /// <summary>
        /// Total de retiros.
        /// </summary>
        public decimal TotalRetiros => _movimientos
            .Where(m => m.TipoMovimiento == TipoMovimientoCaja.Retiro)
            .Sum(m => Math.Abs(m.Monto));

        /// <summary>
        /// Saldo calculado según movimientos.
        /// </summary>
        public decimal SaldoCalculado => FondoInicial + _movimientos.Sum(m => m.Monto);

        // Constructor privado para EF Core
        private Caja() { }

        private Caja(int numeroCaja, string nombre)
        {
            NumeroCaja = numeroCaja;
            Nombre = nombre;
            EstaAbierta = false;
            SaldoActual = 0;
        }

        /// <summary>
        /// Crea una nueva caja.
        /// </summary>
        public static Result<Caja> Crear(int numeroCaja, string nombre)
        {
            if (numeroCaja <= 0)
                return Result.Failure<Caja>("El número de caja debe ser mayor a cero");

            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Failure<Caja>("El nombre de la caja es requerido");

            if (nombre.Length > 100)
                return Result.Failure<Caja>("El nombre no puede exceder 100 caracteres");

            var caja = new Caja(numeroCaja, nombre.Trim());

            return Result.Success(caja);
        }

        // ============================================
        // OPERACIONES DE APERTURA Y CIERRE
        // ============================================

        /// <summary>
        /// Abre la caja con un fondo inicial.
        /// </summary>
        public Result Abrir(decimal fondoInicial, Guid usuarioId)
        {
            if (EstaAbierta)
                return Result.Failure("La caja ya está abierta");

            if (fondoInicial < 0)
                return Result.Failure("El fondo inicial no puede ser negativo");

            if (usuarioId == Guid.Empty)
                return Result.Failure("El Id del usuario es requerido");

            FondoInicial = fondoInicial;
            SaldoActual = fondoInicial;
            EstaAbierta = true;
            FechaApertura = DateTime.UtcNow;
            UsuarioAperturaId = usuarioId;

            // Limpiar datos del cierre anterior
            FechaCierre = null;
            UsuarioCierreId = null;
            SaldoFinalDeclarado = null;
            Diferencia = null;
            ObservacionesCierre = null;
            _movimientos.Clear();

            // Registrar movimiento de apertura
            var movimientoApertura = MovimientoCaja.Crear(
                Id,
                TipoMovimientoCaja.Apertura,
                fondoInicial,
                "Apertura de caja",
                usuarioId);

            if (movimientoApertura.IsSuccess)
                _movimientos.Add(movimientoApertura.Value);

            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Cierra la caja con el saldo declarado por el cajero.
        /// </summary>
        public Result Cerrar(decimal saldoDeclarado, Guid usuarioId, string observaciones = null)
        {
            if (!EstaAbierta)
                return Result.Failure("La caja no está abierta");

            if (saldoDeclarado < 0)
                return Result.Failure("El saldo declarado no puede ser negativo");

            if (usuarioId == Guid.Empty)
                return Result.Failure("El Id del usuario es requerido");

            SaldoFinalDeclarado = saldoDeclarado;
            Diferencia = saldoDeclarado - SaldoCalculado;
            EstaAbierta = false;
            FechaCierre = DateTime.UtcNow;
            UsuarioCierreId = usuarioId;
            ObservacionesCierre = observaciones?.Trim() ?? string.Empty;

            // Registrar movimiento de cierre
            var movimientoCierre = MovimientoCaja.Crear(
                Id,
                TipoMovimientoCaja.Cierre,
                0,
                $"Cierre de caja. Saldo declarado: {saldoDeclarado:C}, Calculado: {SaldoCalculado:C}, Diferencia: {Diferencia:C}",
                usuarioId);

            if (movimientoCierre.IsSuccess)
                _movimientos.Add(movimientoCierre.Value);

            // Si hay diferencia, registrar ajuste
            if (Diferencia != 0)
            {
                var motivoAjuste = Diferencia > 0 ? "Sobrante en caja" : "Faltante en caja";
                var movimientoAjuste = MovimientoCaja.Crear(
                    Id,
                    TipoMovimientoCaja.Ajuste,
                    Diferencia.Value,
                    motivoAjuste,
                    usuarioId);

                if (movimientoAjuste.IsSuccess)
                    _movimientos.Add(movimientoAjuste.Value);
            }

            ActualizarFechaModificacion();

            return Result.Success();
        }

        // ============================================
        // OPERACIONES DE MOVIMIENTOS
        // ============================================

        /// <summary>
        /// Registra un movimiento en la caja.
        /// CRÍTICO: Mantiene sincronizado el saldo.
        /// </summary>
        public Result RegistrarMovimiento(
            TipoMovimientoCaja tipoMovimiento,
            decimal monto,
            string concepto,
            Guid usuarioId,
            string referencia = null)
        {
            if (!EstaAbierta)
                return Result.Failure("La caja debe estar abierta para registrar movimientos");

            if (tipoMovimiento == TipoMovimientoCaja.Apertura ||
                tipoMovimiento == TipoMovimientoCaja.Cierre)
                return Result.Failure("Use los métodos Abrir() y Cerrar() para apertura y cierre");

            // Crear el movimiento
            var resultado = MovimientoCaja.Crear(
                Id,
                tipoMovimiento,
                monto,
                concepto,
                usuarioId,
                referencia);

            if (resultado.IsFailure)
                return Result.Failure(resultado.Error);

            var movimiento = resultado.Value;

            // Validar que el movimiento no deje saldo negativo
            var nuevoSaldo = SaldoActual + monto;
            if (nuevoSaldo < 0)
                return Result.Failure(
                    $"Operación dejaría saldo negativo. Saldo actual: {SaldoActual:C}, Movimiento: {monto:C}");

            // Agregar y actualizar saldo
            _movimientos.Add(movimiento);
            SaldoActual = nuevoSaldo;

            ActualizarFechaModificacion();

            // Validar invariantes
            ValidarInvariantes();

            return Result.Success();
        }

        // ============================================
        // MÉTODOS DE CONVENIENCIA
        // ============================================

        /// <summary>
        /// Registra una venta en la caja.
        /// </summary>
        public Result RegistrarVenta(decimal monto, string folioVenta, Guid usuarioId)
        {
            return RegistrarMovimiento(
                TipoMovimientoCaja.Venta,
                monto,
                $"Venta - Folio: {folioVenta}",
                usuarioId,
                folioVenta);
        }

        /// <summary>
        /// Registra una cancelación de venta.
        /// </summary>
        public Result RegistrarCancelacion(decimal monto, string folioVenta, Guid usuarioId)
        {
            return RegistrarMovimiento(
                TipoMovimientoCaja.CancelacionVenta,
                -monto, // Negativo porque es salida
                $"Cancelación - Folio: {folioVenta}",
                usuarioId,
                folioVenta);
        }

        /// <summary>
        /// Registra un retiro de efectivo.
        /// </summary>
        public Result RegistrarRetiro(decimal monto, string motivo, Guid usuarioId)
        {
            if (monto <= 0)
                return Result.Failure("El monto del retiro debe ser mayor a cero");

            return RegistrarMovimiento(
                TipoMovimientoCaja.Retiro,
                -monto, // Negativo porque es salida
                motivo,
                usuarioId);
        }

        /// <summary>
        /// Registra un depósito de efectivo.
        /// </summary>
        public Result RegistrarDeposito(decimal monto, string motivo, Guid usuarioId)
        {
            if (monto <= 0)
                return Result.Failure("El monto del depósito debe ser mayor a cero");

            return RegistrarMovimiento(
                TipoMovimientoCaja.Deposito,
                monto,
                motivo,
                usuarioId);
        }

        // ============================================
        // VALIDACIÓN DE INVARIANTES
        // ============================================

        private void ValidarInvariantes()
        {
            // INVARIANTE 1: Saldo actual debe coincidir con el calculado
            if (Math.Abs(SaldoActual - SaldoCalculado) > 0.01m) // Tolerancia de 1 centavo
                throw new InvarianteVioladaException(
                    $"Saldo inconsistente. Actual: {SaldoActual:C}, Calculado: {SaldoCalculado:C}");

            // INVARIANTE 2: Saldo nunca negativo
            if (SaldoActual < 0)
                throw new InvarianteVioladaException("Saldo negativo en caja");

            // INVARIANTE 3: Si está cerrada, debe tener fecha de cierre
            if (!EstaAbierta && FechaCierre == null)
                throw new InvarianteVioladaException("Caja cerrada sin fecha de cierre");
        }
    }
}
