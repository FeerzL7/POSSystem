using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.DTOs
{
    /// <summary>
    /// DTO para transferir información de productos.
    /// </summary>
    public class ProductoDto
    {
        public Guid Id { get; set; }
        public string CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal PrecioCosto { get; set; }
        public string Categoria { get; set; }
        public bool GravadoIVA { get; set; }
        public bool Activo { get; set; }
        public int StockDisponible { get; set; }
        public int StockFisico { get; set; }
        public bool StockBajo { get; set; }
    }
}
