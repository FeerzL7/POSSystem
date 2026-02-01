using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se intenta una operación inválida sobre una venta.
    /// </summary>
    public class VentaInvalidaException : DomainException
    {
        public VentaInvalidaException(string message)
            : base(message, "VENTA_INVALIDA")
        {
        }
    }
}
