using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.DTOs
{
    /// <summary>
    /// DTO para transferir información de cajas.
    /// </summary>
    public class CajaDto
    {
        public Guid Id { get; set; }
        public int NumeroCaja { get; set; }
        public string Nombre { get; set; }
        public bool EstaAbierta { get; set; }
        public decimal FondoInicial { get; set; }
        public decimal SaldoActual { get; set; }
        public decimal SaldoCalculado { get; set; }
        public DateTime? FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal TotalCancelaciones { get; set; }
        public decimal TotalRetiros { get; set; }
        public decimal? Diferencia { get; set; }
        public string ObservacionesCierre { get; set; }
    }
}
