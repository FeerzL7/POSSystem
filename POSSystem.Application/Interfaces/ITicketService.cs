using System.Threading.Tasks;
using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;

namespace POSSystem.Application.Interfaces
{
    /// <summary>
    /// Servicio para generar tickets de venta.
    /// </summary>
    public interface ITicketService
    {
        /// <summary>
        /// Genera un ticket de venta.
        /// </summary>
        Task<TicketDto> GenerarTicketVentaAsync(Venta venta);

        /// <summary>
        /// Genera un ticket de cancelación.
        /// </summary>
        Task<TicketDto> GenerarTicketCancelacionAsync(Venta venta);

        /// <summary>
        /// Convierte el ticket a texto plano para impresión.
        /// </summary>
        string ConvertirATexto(TicketDto ticket);

        /// <summary>
        /// Guarda el ticket en el sistema de archivos.
        /// </summary>
        Task<string> GuardarTicketAsync(TicketDto ticket, string nombreArchivo);
    }
}