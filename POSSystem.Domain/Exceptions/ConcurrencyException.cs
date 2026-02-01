using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando hay conflictos de concurrencia.
    /// </summary>
    public class ConcurrencyException : DomainException
    {
        public ConcurrencyException(string message)
            : base(message, "CONCURRENCY_ERROR")
        {
        }

        public ConcurrencyException(string message, Exception innerException)
            : base(message, "CONCURRENCY_ERROR", innerException)
        {
        }
    }
}
