using POSSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.DTOs
{
    /// <summary>
    /// DTO para transferir información de pagos.
    /// </summary>
    public class PagoDto
    {
        public Guid Id { get; set; }
        public decimal Monto { get; set; }
        public TipoPago TipoPago { get; set; }
        public DateTime FechaPago { get; set; }
        public string Referencia { get; set; }
        public decimal Cambio { get; set; }
    }
}
