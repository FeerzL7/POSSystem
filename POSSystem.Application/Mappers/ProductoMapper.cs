using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.Mappers
{
    public static class ProductoMapper
    {
        public static ProductoDto ToDto(this Producto producto, Inventario inventario = null)
        {
            if (producto == null) return null;

            return new ProductoDto
            {
                Id = producto.Id,
                CodigoBarras = producto.CodigoBarras.Valor,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioVenta = producto.PrecioVenta,
                PrecioCosto = producto.PrecioCosto,
                Categoria = producto.Categoria,
                GravadoIVA = producto.GravadoIVA,
                Activo = producto.Activo,
                StockDisponible = inventario?.StockDisponible ?? 0,
                StockFisico = inventario?.StockFisico ?? 0,
                StockBajo = inventario?.StockBajo ?? false
            };
        }
    }
}
