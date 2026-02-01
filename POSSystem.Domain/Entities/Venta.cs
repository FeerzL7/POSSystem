using POSSystem.Domain.Common;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Exceptions;
using POSSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Entities
{
    /// <summary>
    /// AGGREGATE ROOT: Venta
    /// Entidad principal que coordina toda la operación de venta.
    /// Mantiene consistencia de estado y reglas de negocio.
    /// NUNCA debe quedar en estado inconsistente.
    /// </summary>
    public class Venta : EntityBase
    {
        // Colecciones privadas para mantener encapsulamiento
        private readonly List<DetalleVenta> _detalles = new();
        private readonly List<Pago> _pagos = new();

        /// <summary>
        /// Folio único de la venta.
        /// </summary>
        public Folio Folio { get; private set; }

        /// <summary>
        /// Estado actual de la venta.
        /// </summary>
        public EstadoVenta Estado { get; private set; }

        /// <summary>
        /// Id del usuario que realiza la venta.
        /// </summary>
        public Guid UsuarioId { get; private set; }

        /// <summary>
        /// Motivo de cancelación (si aplica).
        /// </summary>
        public string MotivoCancelacion { get; private set; }

        /// <summary>
        /// Fecha de cancelación (si aplica).
        /// </summary>
        public DateTime? FechaCancelacion { get; private set; }

        /// <summary>
        /// Fecha en que se pagó la venta.
        /// </summary>
        public DateTime? FechaPago { get; private set; }

        /// <summary>
        /// Tasa de IVA aplicada (ej: 0.16 para 16%).
        /// </summary>
        public decimal TasaIVA { get; private set; }

        // Exposición de colecciones como solo lectura
        public IReadOnlyCollection<DetalleVenta> Detalles => _detalles.AsReadOnly();
        public IReadOnlyCollection<Pago> Pagos => _pagos.AsReadOnly();

        // Propiedades calculadas (NO se persisten, se calculan en tiempo de ejecución)
        /// <summary>
        /// Subtotal sin impuestos.
        /// </summary>
        public decimal Subtotal => _detalles.Sum(d => d.Subtotal);

        /// <summary>
        /// Total de impuestos.
        /// </summary>
        public decimal Impuestos => _detalles.Sum(d => d.Impuestos);

        /// <summary>
        /// Total de la venta (Subtotal + Impuestos).
        /// </summary>
        public decimal Total => Subtotal + Impuestos;

        /// <summary>
        /// Total pagado.
        /// </summary>
        public decimal TotalPagado => _pagos.Sum(p => p.Monto);

        /// <summary>
        /// Cambio total devuelto.
        /// </summary>
        public decimal CambioTotal => _pagos.Sum(p => p.Cambio);

        /// <summary>
        /// Cantidad total de productos.
        /// </summary>
        public int CantidadProductos => _detalles.Sum(d => d.Cantidad);

        // Constructor privado para EF Core
        private Venta() { }

        private Venta(Folio folio, Guid usuarioId, decimal tasaIVA)
        {
            Folio = folio;
            Estado = EstadoVenta.Nueva;
            UsuarioId = usuarioId;
            TasaIVA = tasaIVA;
        }

        /// <summary>
        /// Crea una nueva venta.
        /// </summary>
        public static Result<Venta> Crear(Folio folio, Guid usuarioId, decimal tasaIVA = 0.16m)
        {
            if (folio == null)
                return Result.Failure<Venta>("El folio es requerido");

            if (usuarioId == Guid.Empty)
                return Result.Failure<Venta>("El Id del usuario es requerido");

            if (tasaIVA < 0 || tasaIVA > 1)
                return Result.Failure<Venta>("La tasa de IVA debe estar entre 0 y 1");

            var venta = new Venta(folio, usuarioId, tasaIVA);

            return Result.Success(venta);
        }

        // ============================================
        // OPERACIONES SOBRE DETALLES
        // ============================================

        /// <summary>
        /// Agrega un producto a la venta o incrementa su cantidad si ya existe.
        /// REGLA: Solo en estado Nueva o Abierta.
        /// </summary>
        public Result AgregarProducto(Producto producto, int cantidad = 1)
        {
            // Validar transición de estado
            if (Estado == EstadoVenta.Pagada)
                return Result.Failure("No se puede modificar una venta pagada");

            if (Estado == EstadoVenta.Cancelada)
                return Result.Failure("No se puede modificar una venta cancelada");

            // Validar datos
            if (producto == null)
                return Result.Failure("El producto es requerido");

            if (cantidad <= 0)
                return Result.Failure("La cantidad debe ser mayor a cero");

            // Buscar si ya existe el producto
            var detalleExistente = _detalles.FirstOrDefault(d => d.ProductoId == producto.Id);

            if (detalleExistente != null)
            {
                // Incrementar cantidad
                var resultadoIncremento = detalleExistente.IncrementarCantidad(cantidad);
                if (resultadoIncremento.IsFailure)
                    return resultadoIncremento;
            }
            else
            {
                // Crear nuevo detalle
                var resultadoDetalle = DetalleVenta.Crear(Id, producto, cantidad, TasaIVA);
                if (resultadoDetalle.IsFailure)
                    return Result.Failure(resultadoDetalle.Error);

                _detalles.Add(resultadoDetalle.Value);
            }

            // Cambiar estado si es necesario
            if (Estado == EstadoVenta.Nueva)
                Estado = EstadoVenta.Abierta;

            ActualizarFechaModificacion();

            // Validar invariantes después de la operación
            ValidarInvariantes();

            return Result.Success();
        }

        /// <summary>
        /// Quita un producto de la venta por completo.
        /// </summary>
        public Result QuitarProducto(Guid productoId)
        {
            // Validar estado
            if (Estado == EstadoVenta.Pagada)
                return Result.Failure("No se puede modificar una venta pagada");

            if (Estado == EstadoVenta.Cancelada)
                return Result.Failure("No se puede modificar una venta cancelada");

            // Buscar el detalle
            var detalle = _detalles.FirstOrDefault(d => d.ProductoId == productoId);
            if (detalle == null)
                return Result.Failure("El producto no existe en la venta");

            // Eliminar
            _detalles.Remove(detalle);

            // Si ya no hay productos, volver a estado Nueva
            if (_detalles.Count == 0)
                Estado = EstadoVenta.Nueva;

            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Modifica la cantidad de un producto en la venta.
        /// </summary>
        public Result ModificarCantidadProducto(Guid productoId, int nuevaCantidad)
        {
            // Validar estado
            if (Estado == EstadoVenta.Pagada)
                return Result.Failure("No se puede modificar una venta pagada");

            if (Estado == EstadoVenta.Cancelada)
                return Result.Failure("No se puede modificar una venta cancelada");

            // Buscar el detalle
            var detalle = _detalles.FirstOrDefault(d => d.ProductoId == productoId);
            if (detalle == null)
                return Result.Failure("El producto no existe en la venta");

            // Establecer nueva cantidad
            var resultado = detalle.EstablecerCantidad(nuevaCantidad);
            if (resultado.IsFailure)
                return resultado;

            ActualizarFechaModificacion();

            return Result.Success();
        }

        // ============================================
        // OPERACIONES DE PAGO
        // ============================================

        /// <summary>
        /// Registra un pago en la venta.
        /// REGLA: Solo en estado Abierta.
        /// TRANSICIÓN: Abierta → Pagada
        /// </summary>
        public Result RegistrarPago(decimal monto, TipoPago tipoPago, string referencia = null)
        {
            // Validar transición
            if (Estado != EstadoVenta.Abierta)
                return Result.Failure($"No se puede pagar una venta en estado {Estado}");

            // Validar que haya productos
            if (_detalles.Count == 0)
                return Result.Failure("No se puede pagar una venta sin productos");

            // Validar monto
            if (monto < Total)
                return Result.Failure($"Pago insuficiente. Falta: {(Total - monto):C}");

            // Crear el pago
            var resultadoPago = Pago.Crear(Id, monto, tipoPago, Total, referencia);
            if (resultadoPago.IsFailure)
                return Result.Failure(resultadoPago.Error);

            _pagos.Add(resultadoPago.Value);

            // Cambiar estado
            Estado = EstadoVenta.Pagada;
            FechaPago = DateTime.UtcNow;

            ActualizarFechaModificacion();

            // Validar invariantes
            ValidarInvariantes();

            return Result.Success();
        }

        // ============================================
        // OPERACIONES DE CANCELACIÓN
        // ============================================

        /// <summary>
        /// Cancela la venta antes del pago.
        /// TRANSICIÓN: Nueva/Abierta → Cancelada
        /// </summary>
        public Result Cancelar(string motivo)
        {
            // Validar transición
            if (Estado == EstadoVenta.Cancelada)
                return Result.Failure("La venta ya está cancelada");

            if (Estado == EstadoVenta.Pagada)
                return Result.Failure("Use ReversarVenta para cancelar ventas pagadas");

            // Validar motivo
            if (string.IsNullOrWhiteSpace(motivo))
                return Result.Failure("Debe proporcionar un motivo de cancelación");

            Estado = EstadoVenta.Cancelada;
            MotivoCancelacion = motivo.Trim();
            FechaCancelacion = DateTime.UtcNow;

            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Reversa una venta ya pagada (cancelación posterior al pago).
        /// TRANSICIÓN: Pagada → Cancelada
        /// NOTA: Esta operación debe ir acompañada de reverso de inventario y caja.
        /// </summary>
        public Result Reversar(string motivo)
        {
            // Validar transición
            if (Estado != EstadoVenta.Pagada)
                return Result.Failure("Solo se pueden reversar ventas pagadas");

            if (Estado == EstadoVenta.Cancelada)
                return Result.Failure("La venta ya está cancelada");

            // Validar motivo
            if (string.IsNullOrWhiteSpace(motivo))
                return Result.Failure("Debe proporcionar un motivo de cancelación");

            Estado = EstadoVenta.Cancelada;
            MotivoCancelacion = motivo.Trim();
            FechaCancelacion = DateTime.UtcNow;

            ActualizarFechaModificacion();

            return Result.Success();
        }

        // ============================================
        // VALIDACIÓN DE INVARIANTES
        // ============================================

        /// <summary>
        /// Valida que la venta siempre esté en un estado consistente.
        /// CRÍTICO: Llamar después de cada operación que modifique el estado.
        /// </summary>
        private void ValidarInvariantes()
        {
            // INVARIANTE 1: Venta pagada debe tener pago suficiente
            if (Estado == EstadoVenta.Pagada && TotalPagado < Total)
                throw new InvarianteVioladaException("Venta pagada sin pago completo");

            // INVARIANTE 2: Venta pagada/cancelada debe tener productos
            if ((Estado == EstadoVenta.Pagada || Estado == EstadoVenta.Cancelada) && _detalles.Count == 0)
                throw new InvarianteVioladaException("Venta finalizada sin productos");

            // INVARIANTE 3: No puede haber cantidades negativas
            if (_detalles.Any(d => d.Cantidad <= 0))
                throw new InvarianteVioladaException("Cantidad negativa o cero en detalle");

            // INVARIANTE 4: No puede haber precios negativos
            if (_detalles.Any(d => d.PrecioUnitario < 0))
                throw new InvarianteVioladaException("Precio negativo en detalle");

            // INVARIANTE 5: Venta pagada debe tener al menos un pago
            if (Estado == EstadoVenta.Pagada && _pagos.Count == 0)
                throw new InvarianteVioladaException("Venta pagada sin registro de pago");
        }
    }
}
