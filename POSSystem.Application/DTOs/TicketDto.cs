using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.DTOs
{
    /// <summary>
    /// DTO para generar tickets de venta.
    /// </summary>
    public class TicketDto
    {
        public string NombreComercio { get; set; }
        public string DireccionComercio { get; set; }
        public string TelefonoComercio { get; set; }
        public string RFC { get; set; }

        public string Folio { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }

        public List<LineaTicket> Productos { get; set; } = new();

        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }

        public string FormaPago { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal Cambio { get; set; }

        public string Cajero { get; set; }
        public string MensajeFinal { get; set; }
    }

    public class LineaTicket
    {
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Importe { get; set; }
    }
}
