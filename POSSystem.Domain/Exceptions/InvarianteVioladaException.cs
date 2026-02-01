using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se viola una invariante de dominio.
    /// Indica un error crítico en la lógica del sistema.
    /// </summary>
    public class InvarianteVioladaException : DomainException
    {
        public InvarianteVioladaException(string message)
            : base($"Invariante violada: {message}", "INVARIANTE_VIOLADA")
        {
        }
    }
}
