using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.Mappers
{
    /// <summary>
    /// Mapea entre entidades de dominio y DTOs.
    /// </summary>
    public static class VentaMapper
    {
        public static VentaDto ToDto(this Venta venta)
        {
            if (venta == null) return null;

            return new VentaDto
            {
                Id = venta.Id,
                Folio = venta.Folio.Valor,
                Estado = venta.Estado,
                FechaCreacion = venta.FechaCreacion,
                FechaPago = venta.FechaPago,
                FechaCancelacion = venta.FechaCancelacion,
                MotivoCancelacion = venta.MotivoCancelacion,
                Subtotal = venta.Subtotal,
                Impuestos = venta.Impuestos,
                Total = venta.Total,
                TotalPagado = venta.TotalPagado,
                Cambio = venta.CambioTotal,
                CantidadProductos = venta.CantidadProductos,
                Detalles = venta.Detalles.Select(d => d.ToDto()).ToList(),
                Pagos = venta.Pagos.Select(p => p.ToDto()).ToList()
            };
        }

        public static DetalleVentaDto ToDto(this DetalleVenta detalle)
        {
            if (detalle == null) return null;

            return new DetalleVentaDto
            {
                Id = detalle.Id,
                ProductoId = detalle.ProductoId,
                CodigoBarras = detalle.CodigoBarras,
                NombreProducto = detalle.NombreProducto,
                Cantidad = detalle.Cantidad,
                PrecioUnitario = detalle.PrecioUnitario,
                Subtotal = detalle.Subtotal,
                Impuestos = detalle.Impuestos,
                Total = detalle.Total,
                GravadoIVA = detalle.GravadoIVA
            };
        }

        public static PagoDto ToDto(this Pago pago)
        {
            if (pago == null) return null;

            return new PagoDto
            {
                Id = pago.Id,
                Monto = pago.Monto,
                TipoPago = pago.TipoPago,
                FechaPago = pago.FechaPago,
                Referencia = pago.Referencia,
                Cambio = pago.Cambio
            };
        }
    }
}
