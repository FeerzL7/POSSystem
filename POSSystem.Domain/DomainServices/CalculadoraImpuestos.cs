using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.DomainServices
{
    /// <summary>
    /// Servicio de dominio para cálculo de impuestos.
    /// Centraliza la lógica de cálculo de IVA y otros impuestos.
    /// </summary>
    public class CalculadoraImpuestos
    {
        /// <summary>
        /// Tasa de IVA estándar en México (16%).
        /// </summary>
        public const decimal TASA_IVA_ESTANDAR = 0.16m;

        /// <summary>
        /// Tasa de IVA reducida (0% para productos de la canasta básica).
        /// </summary>
        public const decimal TASA_IVA_CERO = 0.0m;

        /// <summary>
        /// Calcula el IVA sobre un subtotal.
        /// </summary>
        /// <param name="subtotal">Subtotal sin impuestos</param>
        /// <param name="tasaIVA">Tasa de IVA a aplicar (default 16%)</param>
        /// <returns>Monto del IVA calculado</returns>
        public static decimal CalcularIVA(decimal subtotal, decimal tasaIVA = TASA_IVA_ESTANDAR)
        {
            if (subtotal < 0)
                throw new ArgumentException("El subtotal no puede ser negativo", nameof(subtotal));

            if (tasaIVA < 0 || tasaIVA > 1)
                throw new ArgumentException("La tasa de IVA debe estar entre 0 y 1", nameof(tasaIVA));

            return Math.Round(subtotal * tasaIVA, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calcula el total incluyendo IVA.
        /// </summary>
        /// <param name="subtotal">Subtotal sin impuestos</param>
        /// <param name="tasaIVA">Tasa de IVA a aplicar</param>
        /// <returns>Total con IVA incluido</returns>
        public static decimal CalcularTotalConIVA(decimal subtotal, decimal tasaIVA = TASA_IVA_ESTANDAR)
        {
            var iva = CalcularIVA(subtotal, tasaIVA);
            return subtotal + iva;
        }

        /// <summary>
        /// Desglosa el IVA de un total que ya incluye impuestos.
        /// </summary>
        /// <param name="totalConIVA">Total con IVA incluido</param>
        /// <param name="tasaIVA">Tasa de IVA aplicada</param>
        /// <returns>Tupla con (Subtotal, IVA)</returns>
        public static (decimal Subtotal, decimal IVA) DesglosarIVA(
            decimal totalConIVA,
            decimal tasaIVA = TASA_IVA_ESTANDAR)
        {
            if (totalConIVA < 0)
                throw new ArgumentException("El total no puede ser negativo", nameof(totalConIVA));

            if (tasaIVA < 0 || tasaIVA > 1)
                throw new ArgumentException("La tasa de IVA debe estar entre 0 y 1", nameof(tasaIVA));

            // Fórmula: Subtotal = Total / (1 + TasaIVA)
            var subtotal = Math.Round(totalConIVA / (1 + tasaIVA), 2, MidpointRounding.AwayFromZero);
            var iva = totalConIVA - subtotal;

            return (subtotal, iva);
        }

        /// <summary>
        /// Calcula el IVA de múltiples productos con diferentes tasas.
        /// </summary>
        /// <param name="items">Lista de (Subtotal, TasaIVA)</param>
        /// <returns>IVA total calculado</returns>
        public static decimal CalcularIVAMultiple(params (decimal Subtotal, decimal TasaIVA)[] items)
        {
            decimal ivaTotal = 0;

            foreach (var item in items)
            {
                ivaTotal += CalcularIVA(item.Subtotal, item.TasaIVA);
            }

            return ivaTotal;
        }

        /// <summary>
        /// Redondea un monto al múltiplo de centavos más cercano.
        /// Útil para ajustar totales de venta.
        /// </summary>
        /// <param name="monto">Monto a redondear</param>
        /// <param name="multiplo">Múltiplo de redondeo (default 0.05 = 5 centavos)</param>
        /// <returns>Monto redondeado</returns>
        public static decimal RedondearMonto(decimal monto, decimal multiplo = 0.05m)
        {
            if (multiplo <= 0)
                throw new ArgumentException("El múltiplo debe ser mayor a cero", nameof(multiplo));

            return Math.Round(monto / multiplo, MidpointRounding.AwayFromZero) * multiplo;
        }

        /// <summary>
        /// Determina si un producto debe llevar IVA según su categoría.
        /// Simplificado: solo para demostración, en producción usar catálogo SAT.
        /// </summary>
        /// <param name="categoria">Categoría del producto</param>
        /// <returns>True si debe llevar IVA estándar</returns>
        public static bool DebeAplicarIVAEstandar(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                return true; // Por defecto, sí lleva IVA

            // Categorías exentas o con tasa 0%
            var categoriasExentas = new[]
            {
                "ALIMENTOS_BASICOS",
                "MEDICINAS",
                "LIBROS",
                "PERIODICOS"
            };

            return !Array.Exists(categoriasExentas,
                c => c.Equals(categoria.ToUpperInvariant().Trim()));
        }

        /// <summary>
        /// Obtiene la tasa de IVA aplicable según la categoría del producto.
        /// </summary>
        /// <param name="categoria">Categoría del producto</param>
        /// <returns>Tasa de IVA aplicable</returns>
        public static decimal ObtenerTasaIVAPorCategoria(string categoria)
        {
            return DebeAplicarIVAEstandar(categoria) ? TASA_IVA_ESTANDAR : TASA_IVA_CERO;
        }
    }
}
