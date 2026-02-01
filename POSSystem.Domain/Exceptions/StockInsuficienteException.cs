using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando no hay suficiente stock disponible.
    /// </summary>
    public class StockInsuficienteException : DomainException
    {
        public Guid ProductoId { get; }
        public int StockDisponible { get; }
        public int CantidadSolicitada { get; }

        public StockInsuficienteException(Guid productoId, int stockDisponible, int cantidadSolicitada)
            : base($"Stock insuficiente. Disponible: {stockDisponible}, Solicitado: {cantidadSolicitada}", "STOCK_INSUFICIENTE")
        {
            ProductoId = productoId;
            StockDisponible = stockDisponible;
            CantidadSolicitada = cantidadSolicitada;
        }
    }
}
