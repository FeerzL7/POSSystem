using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando no se encuentra un producto en el sistema.
    /// </summary>
    public class ProductoNoEncontradoException : DomainException
    {
        public string CodigoBarras { get; }

        public ProductoNoEncontradoException(string codigoBarras)
            : base($"No se encontró el producto con código de barras: {codigoBarras}", "PRODUCTO_NO_ENCONTRADO")
        {
            CodigoBarras = codigoBarras;
        }
    }
}
