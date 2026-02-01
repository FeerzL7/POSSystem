using POSSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un producto dentro de una venta.
    /// No puede existir independientemente de una Venta (Aggregate).
    /// </summary>
    public class DetalleVenta : EntityBase
    {
        /// <summary>
        /// Id de la venta a la que pertenece (foreign key).
        /// </summary>
        public Guid VentaId { get; private set; }

        /// <summary>
        /// Id del producto vendido.
        /// </summary>
        public Guid ProductoId { get; private set; }

        /// <summary>
        /// Nombre del producto (snapshot en el momento de la venta).
        /// Importante: aunque el producto cambie después, mantenemos el nombre de ese momento.
        /// </summary>
        public string NombreProducto { get; private set; }

        /// <summary>
        /// Código de barras (snapshot).
        /// </summary>
        public string CodigoBarras { get; private set; }

        /// <summary>
        /// Cantidad vendida.
        /// </summary>
        public int Cantidad { get; private set; }

        /// <summary>
        /// Precio unitario en el momento de la venta (snapshot).
        /// </summary>
        public decimal PrecioUnitario { get; private set; }

        /// <summary>
        /// Indica si el producto estaba gravado con IVA.
        /// </summary>
        public bool GravadoIVA { get; private set; }

        /// <summary>
        /// Tasa de IVA aplicada (ej: 0.16 para 16%).
        /// </summary>
        public decimal TasaIVA { get; private set; }

        // Propiedades calculadas
        /// <summary>
        /// Subtotal sin impuestos (Cantidad * PrecioUnitario).
        /// </summary>
        public decimal Subtotal => Cantidad * PrecioUnitario;

        /// <summary>
        /// Impuestos calculados.
        /// </summary>
        public decimal Impuestos => GravadoIVA ? Subtotal * TasaIVA : 0;

        /// <summary>
        /// Total del detalle (Subtotal + Impuestos).
        /// </summary>
        public decimal Total => Subtotal + Impuestos;

        // Navegación
        public Producto Producto { get; private set; }

        // Constructor privado para EF Core
        private DetalleVenta() { }

        private DetalleVenta(
            Guid ventaId,
            Producto producto,
            int cantidad,
            decimal tasaIVA)
        {
            VentaId = ventaId;
            ProductoId = producto.Id;
            NombreProducto = producto.Nombre;
            CodigoBarras = producto.CodigoBarras.Valor;
            PrecioUnitario = producto.PrecioVenta;
            GravadoIVA = producto.GravadoIVA;
            TasaIVA = tasaIVA;
            Cantidad = cantidad;
        }

        /// <summary>
        /// Crea un nuevo detalle de venta.
        /// </summary>
        public static Result<DetalleVenta> Crear(
            Guid ventaId,
            Producto producto,
            int cantidad,
            decimal tasaIVA = 0.16m)
        {
            // Validaciones
            if (ventaId == Guid.Empty)
                return Result.Failure<DetalleVenta>("El Id de la venta es requerido");

            if (producto == null)
                return Result.Failure<DetalleVenta>("El producto es requerido");

            if (!producto.Activo)
                return Result.Failure<DetalleVenta>("No se puede vender un producto inactivo");

            if (cantidad <= 0)
                return Result.Failure<DetalleVenta>("La cantidad debe ser mayor a cero");

            if (cantidad > 9999)
                return Result.Failure<DetalleVenta>("La cantidad no puede exceder 9999 unidades");

            if (tasaIVA < 0 || tasaIVA > 1)
                return Result.Failure<DetalleVenta>("La tasa de IVA debe estar entre 0 y 1");

            var detalle = new DetalleVenta(ventaId, producto, cantidad, tasaIVA);

            return Result.Success(detalle);
        }

        /// <summary>
        /// Incrementa la cantidad del detalle.
        /// </summary>
        public Result IncrementarCantidad(int cantidadAdicional = 1)
        {
            if (cantidadAdicional <= 0)
                return Result.Failure("La cantidad adicional debe ser mayor a cero");

            var nuevaCantidad = Cantidad + cantidadAdicional;

            if (nuevaCantidad > 9999)
                return Result.Failure("La cantidad no puede exceder 9999 unidades");

            Cantidad = nuevaCantidad;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Decrementa la cantidad del detalle.
        /// </summary>
        public Result DecrementarCantidad(int cantidadAReducir = 1)
        {
            if (cantidadAReducir <= 0)
                return Result.Failure("La cantidad a reducir debe ser mayor a cero");

            var nuevaCantidad = Cantidad - cantidadAReducir;

            if (nuevaCantidad <= 0)
                return Result.Failure("La cantidad no puede ser cero o negativa. Elimine el detalle.");

            Cantidad = nuevaCantidad;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Establece una cantidad específica.
        /// </summary>
        public Result EstablecerCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                return Result.Failure("La cantidad debe ser mayor a cero");

            if (nuevaCantidad > 9999)
                return Result.Failure("La cantidad no puede exceder 9999 unidades");

            Cantidad = nuevaCantidad;
            ActualizarFechaModificacion();

            return Result.Success();
        }
    }
}
