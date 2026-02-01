using POSSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando se intenta una transición de estado inválida.
    /// </summary>
    public class TransicionInvalidaException : DomainException
    {
        public EstadoVenta EstadoActual { get; }
        public EstadoVenta EstadoDeseado { get; }

        public TransicionInvalidaException(EstadoVenta estadoActual, EstadoVenta estadoDeseado)
            : base($"Transición inválida: {estadoActual} → {estadoDeseado}", "TRANSICION_INVALIDA")
        {
            EstadoActual = estadoActual;
            EstadoDeseado = estadoDeseado;
        }

        public TransicionInvalidaException(string message)
            : base(message, "TRANSICION_INVALIDA")
        {
        }
    }
}
