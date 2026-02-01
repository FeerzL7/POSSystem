using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.DTOs
{
    /// <summary>
    /// DTO para transferir información de detalles de venta.
    /// </summary>
    public class DetalleVentaDto
    {
        public Guid Id { get; set; }
        public Guid ProductoId { get; set; }
        public string CodigoBarras { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public bool GravadoIVA { get; set; }
    }
}
