using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción base para todas las excepciones de dominio.
    /// Representa violaciones de reglas de negocio.
    /// </summary>
    public abstract class DomainException : Exception
    {
        /// <summary>
        /// Código de error único para identificación.
        /// </summary>
        public string ErrorCode { get; }

        protected DomainException(string message, string errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        protected DomainException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
