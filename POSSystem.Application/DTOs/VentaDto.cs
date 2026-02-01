using POSSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.DTOs
{
    /// <summary>
    /// DTO para transferir información de ventas.
    /// </summary>
    public class VentaDto
    {
        public Guid Id { get; set; }
        public string Folio { get; set; }
        public EstadoVenta Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime? FechaCancelacion { get; set; }
        public string MotivoCancelacion { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal Cambio { get; set; }
        public int CantidadProductos { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; } = new();
        public List<PagoDto> Pagos { get; set; } = new();
    }
}
