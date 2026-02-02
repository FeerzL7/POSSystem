using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Application.Interfaces;
using POSSystem.Domain.Entities;

namespace POSSystem.Infrastructure.Services
{
    /// <summary>
    /// Servicio para generar tickets de venta.
    /// </summary>
    public class TicketService : ITicketService
    {
        private readonly ILogger<TicketService> _logger;

        // Configuración del comercio (debería venir de configuración o BD)
        private const string NOMBRE_COMERCIO = "COMERCIO OXXO-STYLE";
        private const string DIRECCION_COMERCIO = "Av. Principal #123, Col. Centro";
        private const string TELEFONO_COMERCIO = "Tel: (123) 456-7890";
        private const string RFC_COMERCIO = "RFC: XAXX010101000";

        public TicketService(ILogger<TicketService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Genera un ticket de venta.
        /// </summary>
        public async Task<TicketDto> GenerarTicketVentaAsync(Venta venta)
        {
            try
            {
                if (venta == null)
                    throw new ArgumentNullException(nameof(venta));

                var ticket = new TicketDto
                {
                    NombreComercio = NOMBRE_COMERCIO,
                    DireccionComercio = DIRECCION_COMERCIO,
                    TelefonoComercio = TELEFONO_COMERCIO,
                    RFC = RFC_COMERCIO,
                    Folio = venta.Folio.Valor,
                    Fecha = venta.FechaPago ?? venta.FechaCreacion,
                    Estado = venta.Estado.ToString(),
                    Subtotal = venta.Subtotal,
                    Impuestos = venta.Impuestos,
                    Total = venta.Total,
                    Productos = venta.Detalles.Select(d => new LineaTicket
                    {
                        Descripcion = d.NombreProducto,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Importe = d.Total
                    }).ToList()
                };

                // Información del pago
                if (venta.Pagos.Any())
                {
                    var pago = venta.Pagos.First();
                    ticket.FormaPago = pago.TipoPago.ToString();
                    ticket.MontoPagado = pago.Monto;
                    ticket.Cambio = pago.Cambio;
                }

                ticket.MensajeFinal = "¡Gracias por su compra!\nVuelva pronto";

                _logger.LogInformation("Ticket generado para venta {Folio}", venta.Folio.Valor);

                return await Task.FromResult(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando ticket para venta {VentaId}", venta?.Id);
                throw;
            }
        }

        /// <summary>
        /// Genera un ticket de cancelación.
        /// </summary>
        public async Task<TicketDto> GenerarTicketCancelacionAsync(Venta venta)
        {
            try
            {
                if (venta == null)
                    throw new ArgumentNullException(nameof(venta));

                var ticket = await GenerarTicketVentaAsync(venta);
                ticket.Estado = "CANCELADA";
                ticket.MensajeFinal = $"VENTA CANCELADA\nMotivo: {venta.MotivoCancelacion}\n" +
                                     $"Fecha cancelación: {venta.FechaCancelacion:dd/MM/yyyy HH:mm}";

                _logger.LogInformation("Ticket de cancelación generado para venta {Folio}", venta.Folio.Valor);

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando ticket de cancelación para venta {VentaId}", venta?.Id);
                throw;
            }
        }

        /// <summary>
        /// Convierte el ticket a texto plano para impresión.
        /// Formato tipo OXXO con ancho de 40 caracteres.
        /// </summary>
        public string ConvertirATexto(TicketDto ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            const int ANCHO = 40;
            var sb = new StringBuilder();

            // Encabezado
            sb.AppendLine(Centrar(ticket.NombreComercio, ANCHO));
            sb.AppendLine(Centrar(ticket.DireccionComercio, ANCHO));
            sb.AppendLine(Centrar(ticket.TelefonoComercio, ANCHO));
            sb.AppendLine(Centrar(ticket.RFC, ANCHO));
            sb.AppendLine(new string('=', ANCHO));

            // Información de venta
            sb.AppendLine($"Folio: {ticket.Folio}");
            sb.AppendLine($"Fecha: {ticket.Fecha:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Estado: {ticket.Estado}");
            sb.AppendLine(new string('-', ANCHO));

            // Productos
            foreach (var producto in ticket.Productos)
            {
                // Línea 1: Descripción
                sb.AppendLine(producto.Descripcion);

                // Línea 2: Cantidad x Precio = Importe
                var linea = $"  {producto.Cantidad} x {producto.PrecioUnitario:C}";
                var importe = producto.Importe.ToString("C");
                sb.AppendLine(AlinearDerecha(linea, importe, ANCHO));
            }

            sb.AppendLine(new string('-', ANCHO));

            // Totales
            sb.AppendLine(AlinearDerecha("Subtotal:", ticket.Subtotal.ToString("C"), ANCHO));
            sb.AppendLine(AlinearDerecha("IVA:", ticket.Impuestos.ToString("C"), ANCHO));
            sb.AppendLine(new string('=', ANCHO));
            sb.AppendLine(AlinearDerecha("TOTAL:", ticket.Total.ToString("C"), ANCHO));
            sb.AppendLine(new string('=', ANCHO));

            // Información de pago
            if (!string.IsNullOrEmpty(ticket.FormaPago))
            {
                sb.AppendLine();
                sb.AppendLine($"Forma de pago: {ticket.FormaPago}");
                sb.AppendLine(AlinearDerecha("Pago con:", ticket.MontoPagado.ToString("C"), ANCHO));
                sb.AppendLine(AlinearDerecha("Cambio:", ticket.Cambio.ToString("C"), ANCHO));
            }

            // Mensaje final
            sb.AppendLine();
            sb.AppendLine(new string('=', ANCHO));
            foreach (var linea in ticket.MensajeFinal.Split('\n'))
            {
                sb.AppendLine(Centrar(linea, ANCHO));
            }
            sb.AppendLine(new string('=', ANCHO));

            return sb.ToString();
        }

        /// <summary>
        /// Guarda el ticket en el sistema de archivos.
        /// </summary>
        public async Task<string> GuardarTicketAsync(TicketDto ticket, string nombreArchivo)
        {
            try
            {
                var directorio = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "POSSystem",
                    "Tickets");

                Directory.CreateDirectory(directorio);

                var nombreCompleto = $"{nombreArchivo}_{ticket.Folio}_{DateTime.Now:yyyyMMddHHmmss}.txt";
                var rutaCompleta = Path.Combine(directorio, nombreCompleto);

                var contenido = ConvertirATexto(ticket);
                await File.WriteAllTextAsync(rutaCompleta, contenido, Encoding.UTF8);

                _logger.LogInformation("Ticket guardado en: {Ruta}", rutaCompleta);

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando ticket {Folio}", ticket?.Folio);
                throw;
            }
        }

        // Métodos auxiliares de formateo
        private string Centrar(string texto, int ancho)
        {
            if (string.IsNullOrEmpty(texto))
                return new string(' ', ancho);

            if (texto.Length >= ancho)
                return texto.Substring(0, ancho);

            int espaciosIzquierda = (ancho - texto.Length) / 2;
            return new string(' ', espaciosIzquierda) + texto;
        }

        private string AlinearDerecha(string etiqueta, string valor, int ancho)
        {
            var longitud = etiqueta.Length + valor.Length;
            if (longitud >= ancho)
                return etiqueta + valor;

            var espacios = ancho - longitud;
            return etiqueta + new string(' ', espacios) + valor;
        }
    }
}