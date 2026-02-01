using POSSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa un código de barras.
    /// Soporta formatos EAN-13, EAN-8, UPC, Code128.
    /// Inmutable y con validación.
    /// </summary>
    public class CodigoBarras : IEquatable<CodigoBarras>
    {
        public string Valor { get; }

        private CodigoBarras(string valor)
        {
            Valor = valor;
        }

        public static Result<CodigoBarras> Crear(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Result.Failure<CodigoBarras>("El código de barras no puede estar vacío");

            // Limpiar espacios
            valor = valor.Trim();

            // Validar que solo contenga números (simplificado, puede extenderse)
            if (!EsFormatoValido(valor))
                return Result.Failure<CodigoBarras>("El código de barras debe contener solo números");

            // Validar longitud (EAN-13: 13, EAN-8: 8, UPC: 12)
            if (!EsLongitudValida(valor))
                return Result.Failure<CodigoBarras>("Longitud de código de barras inválida");

            return Result.Success(new CodigoBarras(valor));
        }

        private static bool EsFormatoValido(string valor)
        {
            foreach (char c in valor)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private static bool EsLongitudValida(string valor)
        {
            int longitud = valor.Length;
            // Longitudes comunes: 8 (EAN-8), 12 (UPC), 13 (EAN-13)
            // Aceptamos cualquier longitud entre 6 y 20 para flexibilidad
            return longitud >= 6 && longitud <= 20;
        }

        public override string ToString() => Valor;

        public bool Equals(CodigoBarras other)
        {
            if (other is null) return false;
            return Valor == other.Valor;
        }

        public override bool Equals(object obj) => obj is CodigoBarras codigo && Equals(codigo);
        public override int GetHashCode() => Valor.GetHashCode();

        public static bool operator ==(CodigoBarras left, CodigoBarras right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(CodigoBarras left, CodigoBarras right) => !(left == right);
    }
}
