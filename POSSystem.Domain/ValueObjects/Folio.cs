using POSSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa un folio único de venta.
    /// Formato: AAAAMMDD-NNNN (ej: 20240215-0001)
    /// Inmutable y con validación.
    /// </summary>
    public class Folio : IEquatable<Folio>
    {
        public string Valor { get; }

        private Folio(string valor)
        {
            Valor = valor;
        }

        /// <summary>
        /// Genera un nuevo folio basado en la fecha actual y un consecutivo.
        /// </summary>
        public static Result<Folio> Crear(int consecutivo)
        {
            if (consecutivo < 1 || consecutivo > 9999)
                return Result.Failure<Folio>("El consecutivo debe estar entre 1 y 9999");

            var fecha = DateTime.Now.ToString("yyyyMMdd");
            var numero = consecutivo.ToString("D4");
            var valor = $"{fecha}-{numero}";

            return Result.Success(new Folio(valor));
        }

        /// <summary>
        /// Crea un folio desde un string existente (para rehidratación desde BD).
        /// </summary>
        public static Result<Folio> Desde(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Result.Failure<Folio>("El folio no puede estar vacío");

            if (!EsFormatoValido(valor))
                return Result.Failure<Folio>("Formato de folio inválido");

            return Result.Success(new Folio(valor));
        }

        private static bool EsFormatoValido(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            var partes = valor.Split('-');
            if (partes.Length != 2)
                return false;

            // Validar fecha (8 dígitos)
            if (partes[0].Length != 8 || !int.TryParse(partes[0], out _))
                return false;

            // Validar consecutivo (4 dígitos)
            if (partes[1].Length != 4 || !int.TryParse(partes[1], out _))
                return false;

            return true;
        }

        public override string ToString() => Valor;

        // Implementación de igualdad por valor
        public bool Equals(Folio other)
        {
            if (other is null) return false;
            return Valor == other.Valor;
        }

        public override bool Equals(object obj) => obj is Folio folio && Equals(folio);
        public override int GetHashCode() => Valor.GetHashCode();

        public static bool operator ==(Folio left, Folio right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Folio left, Folio right) => !(left == right);
    }
}
