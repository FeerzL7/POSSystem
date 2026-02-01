using POSSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa una cantidad monetaria.
    /// Inmutable, con precisión decimal y validaciones.
    /// </summary>
    public class Dinero : IEquatable<Dinero>, IComparable<Dinero>
    {
        public decimal Monto { get; }
        public string Moneda { get; }

        private Dinero(decimal monto, string moneda = "MXN")
        {
            Monto = Math.Round(monto, 2); // Precisión de 2 decimales
            Moneda = moneda;
        }

        public static Result<Dinero> Crear(decimal monto, string moneda = "MXN")
        {
            if (monto < 0)
                return Result.Failure<Dinero>("El monto no puede ser negativo");

            if (string.IsNullOrWhiteSpace(moneda))
                return Result.Failure<Dinero>("La moneda no puede estar vacía");

            return Result.Success(new Dinero(monto, moneda));
        }

        public static Dinero Cero() => new Dinero(0);

        // Operadores aritméticos
        public static Dinero operator +(Dinero a, Dinero b)
        {
            ValidarMismaMoneda(a, b);
            return new Dinero(a.Monto + b.Monto, a.Moneda);
        }

        public static Dinero operator -(Dinero a, Dinero b)
        {
            ValidarMismaMoneda(a, b);
            var resultado = a.Monto - b.Monto;
            if (resultado < 0)
                throw new InvalidOperationException("El resultado no puede ser negativo");
            return new Dinero(resultado, a.Moneda);
        }

        public static Dinero operator *(Dinero dinero, int multiplicador)
        {
            return new Dinero(dinero.Monto * multiplicador, dinero.Moneda);
        }

        public static Dinero operator *(Dinero dinero, decimal multiplicador)
        {
            return new Dinero(dinero.Monto * multiplicador, dinero.Moneda);
        }

        private static void ValidarMismaMoneda(Dinero a, Dinero b)
        {
            if (a.Moneda != b.Moneda)
                throw new InvalidOperationException("No se pueden operar cantidades de diferentes monedas");
        }

        // Comparaciones
        public static bool operator >(Dinero a, Dinero b)
        {
            ValidarMismaMoneda(a, b);
            return a.Monto > b.Monto;
        }

        public static bool operator <(Dinero a, Dinero b)
        {
            ValidarMismaMoneda(a, b);
            return a.Monto < b.Monto;
        }

        public static bool operator >=(Dinero a, Dinero b)
        {
            ValidarMismaMoneda(a, b);
            return a.Monto >= b.Monto;
        }

        public static bool operator <=(Dinero a, Dinero b)
        {
            ValidarMismaMoneda(a, b);
            return a.Monto <= b.Monto;
        }

        public override string ToString() => $"{Monto:C} {Moneda}";

        public bool Equals(Dinero other)
        {
            if (other is null) return false;
            return Monto == other.Monto && Moneda == other.Moneda;
        }

        public override bool Equals(object obj) => obj is Dinero dinero && Equals(dinero);
        public override int GetHashCode() => HashCode.Combine(Monto, Moneda);

        public int CompareTo(Dinero other)
        {
            if (other == null) return 1;
            ValidarMismaMoneda(this, other);
            return Monto.CompareTo(other.Monto);
        }

        public static bool operator ==(Dinero left, Dinero right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Dinero left, Dinero right) => !(left == right);
    }
}
