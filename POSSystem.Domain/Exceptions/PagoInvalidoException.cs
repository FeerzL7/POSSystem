using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando un pago es inválido o insuficiente.
    /// </summary>
    public class PagoInvalidoException : DomainException
    {
        public decimal MontoRequerido { get; }
        public decimal MontoPagado { get; }

        public PagoInvalidoException(decimal montoRequerido, decimal montoPagado)
            : base($"Pago insuficiente. Requerido: {montoRequerido:C}, Pagado: {montoPagado:C}", "PAGO_INVALIDO")
        {
            MontoRequerido = montoRequerido;
            MontoPagado = montoPagado;
        }

        public PagoInvalidoException(string message)
            : base(message, "PAGO_INVALIDO")
        {
        }
    }
}
