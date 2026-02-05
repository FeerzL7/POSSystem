using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Entities
{
    /// <summary>
    /// Clase base abstracta para todas las entidades del dominio.
    /// Proporciona identidad única (Id) y marca de tiempo.
    /// </summary>
    public abstract class EntityBase
    {
        /// <summary>
        /// Identificador único de la entidad.
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// Fecha de creación de la entidad.
        /// </summary>
        public DateTime FechaCreacion { get; protected set; }

        /// <summary>
        /// Fecha de última modificación.
        /// </summary>
        public DateTime UltimaModificacion { get; protected set; }

        /// <summary>
        /// Token de concurrencia para control optimista.
        /// EF Core lo usa para detectar conflictos.
        /// </summary>
        public byte[] RowVersion { get; protected set; }

        protected EntityBase()
        {
            Id = Guid.NewGuid();
            FechaCreacion = DateTime.UtcNow;
            UltimaModificacion = DateTime.UtcNow;
        }

        protected void ActualizarFechaModificacion()
        {
            UltimaModificacion = DateTime.UtcNow;
        }

        // Igualdad basada en Id
        public override bool Equals(object obj)
        {
            if (obj is not EntityBase other)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (GetType() != other.GetType())
                return false;

            // Si ambos son nuevos (Id por defecto), no son iguales a menos que sean la misma referencia
            if (Id == Guid.Empty || other.Id == Guid.Empty)
                return false;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return (GetType().ToString() + Id).GetHashCode();
        }

        public static bool operator ==(EntityBase? a, EntityBase? b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(EntityBase? a, EntityBase? b)
        {
            return !(a == b);
        }
    }
}
