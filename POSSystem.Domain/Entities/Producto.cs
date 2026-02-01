using POSSystem.Domain.Common;
using POSSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un producto en el catálogo.
    /// Entidad simple, no es Aggregate Root.
    /// </summary>
    public class Producto : EntityBase
    {
        /// <summary>
        /// Código de barras del producto.
        /// </summary>
        public CodigoBarras CodigoBarras { get; private set; }

        /// <summary>
        /// Nombre o descripción del producto.
        /// </summary>
        public string Nombre { get; private set; }

        /// <summary>
        /// Descripción detallada (opcional).
        /// </summary>
        public string Descripcion { get; private set; }

        /// <summary>
        /// Precio unitario de venta.
        /// </summary>
        public decimal PrecioVenta { get; private set; }

        /// <summary>
        /// Precio de compra (costo).
        /// </summary>
        public decimal PrecioCosto { get; private set; }

        /// <summary>
        /// Indica si el producto está activo en el sistema.
        /// </summary>
        public bool Activo { get; private set; }

        /// <summary>
        /// Categoría del producto (opcional).
        /// </summary>
        public string Categoria { get; private set; }

        /// <summary>
        /// Indica si el producto está gravado con IVA.
        /// </summary>
        public bool GravadoIVA { get; private set; }

        // Constructor privado para EF Core
        private Producto() { }

        private Producto(
            CodigoBarras codigoBarras,
            string nombre,
            string descripcion,
            decimal precioVenta,
            decimal precioCosto,
            string categoria,
            bool gravadoIVA)
        {
            CodigoBarras = codigoBarras;
            Nombre = nombre;
            Descripcion = descripcion;
            PrecioVenta = precioVenta;
            PrecioCosto = precioCosto;
            Categoria = categoria;
            GravadoIVA = gravadoIVA;
            Activo = true;
        }

        /// <summary>
        /// Crea un nuevo producto con validaciones.
        /// </summary>
        public static Result<Producto> Crear(
            CodigoBarras codigoBarras,
            string nombre,
            decimal precioVenta,
            decimal precioCosto,
            string descripcion = "",
            string categoria = "",
            bool gravadoIVA = true)
        {
            // Validaciones
            if (codigoBarras == null)
                return Result.Failure<Producto>("El código de barras es requerido");

            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Failure<Producto>("El nombre es requerido");

            if (nombre.Length > 200)
                return Result.Failure<Producto>("El nombre no puede exceder 200 caracteres");

            if (precioVenta <= 0)
                return Result.Failure<Producto>("El precio de venta debe ser mayor a cero");

            if (precioCosto < 0)
                return Result.Failure<Producto>("El precio de costo no puede ser negativo");

            if (precioCosto > precioVenta)
                return Result.Failure<Producto>("El precio de costo no puede ser mayor al precio de venta");

            var producto = new Producto(
                codigoBarras,
                nombre.Trim(),
                descripcion?.Trim() ?? string.Empty,
                precioVenta,
                precioCosto,
                categoria?.Trim() ?? string.Empty,
                gravadoIVA);

            return Result.Success(producto);
        }

        /// <summary>
        /// Actualiza el precio de venta del producto.
        /// </summary>
        public Result ActualizarPrecioVenta(decimal nuevoPrecio)
        {
            if (nuevoPrecio <= 0)
                return Result.Failure("El precio debe ser mayor a cero");

            if (nuevoPrecio < PrecioCosto)
                return Result.Failure("El precio de venta no puede ser menor al costo");

            PrecioVenta = nuevoPrecio;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Actualiza el precio de costo del producto.
        /// </summary>
        public Result ActualizarPrecioCosto(decimal nuevoCosto)
        {
            if (nuevoCosto < 0)
                return Result.Failure("El costo no puede ser negativo");

            if (nuevoCosto > PrecioVenta)
                return Result.Failure("El costo no puede ser mayor al precio de venta");

            PrecioCosto = nuevoCosto;
            ActualizarFechaModificacion();

            return Result.Success();
        }

        /// <summary>
        /// Desactiva el producto (no se puede vender).
        /// </summary>
        public void Desactivar()
        {
            Activo = false;
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Activa el producto (disponible para venta).
        /// </summary>
        public void Activar()
        {
            Activo = true;
            ActualizarFechaModificacion();
        }

        /// <summary>
        /// Actualiza la información del producto.
        /// </summary>
        public Result Actualizar(string nombre, string descripcion, string categoria)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result.Failure("El nombre es requerido");

            if (nombre.Length > 200)
                return Result.Failure("El nombre no puede exceder 200 caracteres");

            Nombre = nombre.Trim();
            Descripcion = descripcion?.Trim() ?? string.Empty;
            Categoria = categoria?.Trim() ?? string.Empty;
            ActualizarFechaModificacion();

            return Result.Success();
        }
    }
}
