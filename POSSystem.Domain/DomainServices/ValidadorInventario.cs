using POSSystem.Domain.Common;
using POSSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.DomainServices
{
    /// <summary>
    /// Servicio de dominio para validaciones de inventario.
    /// Contiene lógica de negocio relacionada con stock.
    /// </summary>
    public class ValidadorInventario
    {
        /// <summary>
        /// Valida que hay stock suficiente para una venta completa.
        /// </summary>
        /// <param name="detalles">Detalles de la venta a validar</param>
        /// <param name="inventarios">Inventarios actuales</param>
        /// <returns>Resultado con lista de productos sin stock</returns>
        public static Result ValidarStockParaVenta(
            IEnumerable<DetalleVenta> detalles,
            IEnumerable<Inventario> inventarios)
        {
            if (detalles == null || !detalles.Any())
                return Result.Failure("No hay productos para validar");

            if (inventarios == null)
                return Result.Failure("No se proporcionaron inventarios");

            var productosInsuficientes = new List<string>();

            foreach (var detalle in detalles)
            {
                var inventario = inventarios.FirstOrDefault(i => i.ProductoId == detalle.ProductoId);

                if (inventario == null)
                {
                    productosInsuficientes.Add(
                        $"{detalle.NombreProducto}: No hay registro de inventario");
                    continue;
                }

                if (inventario.StockDisponible < detalle.Cantidad)
                {
                    productosInsuficientes.Add(
                        $"{detalle.NombreProducto}: Solicitado {detalle.Cantidad}, Disponible {inventario.StockDisponible}");
                }
            }

            if (productosInsuficientes.Any())
            {
                var mensaje = "Stock insuficiente:\n" + string.Join("\n", productosInsuficientes);
                return Result.Failure(mensaje);
            }

            return Result.Success();
        }

        /// <summary>
        /// Valida que un producto tenga stock disponible.
        /// </summary>
        public static Result ValidarStockDisponible(Inventario inventario, int cantidadRequerida)
        {
            if (inventario == null)
                return Result.Failure("Inventario no encontrado");

            if (cantidadRequerida <= 0)
                return Result.Failure("La cantidad requerida debe ser mayor a cero");

            if (inventario.StockDisponible < cantidadRequerida)
                return Result.Failure(
                    $"Stock insuficiente. Disponible: {inventario.StockDisponible}, Requerido: {cantidadRequerida}");

            return Result.Success();
        }

        /// <summary>
        /// Determina si un producto requiere reabastecimiento.
        /// </summary>
        public static bool RequiereReabastecimiento(Inventario inventario)
        {
            if (inventario == null)
                return false;

            return inventario.StockBajo;
        }

        /// <summary>
        /// Calcula la cantidad sugerida para reabastecimiento.
        /// </summary>
        public static int CalcularCantidadReabastecimiento(Inventario inventario)
        {
            if (inventario == null)
                return 0;

            if (!inventario.StockBajo)
                return 0;

            // Fórmula simple: Llenar hasta el stock máximo
            return inventario.StockMaximo - inventario.StockFisico;
        }

        /// <summary>
        /// Valida que no haya discrepancias entre stock físico y reservas.
        /// </summary>
        public static Result ValidarConsistenciaInventario(Inventario inventario)
        {
            if (inventario == null)
                return Result.Failure("Inventario no encontrado");

            // Validar que las reservas no excedan el stock físico
            if (inventario.CantidadReservada > inventario.StockFisico)
                return Result.Failure(
                    $"Inconsistencia: Reservas ({inventario.CantidadReservada}) " +
                    $"exceden stock físico ({inventario.StockFisico})");

            // Validar que el stock disponible sea lógico
            if (inventario.StockDisponible < 0)
                return Result.Failure("Stock disponible negativo");

            return Result.Success();
        }

        /// <summary>
        /// Obtiene productos con stock crítico (bajo mínimo).
        /// </summary>
        public static IEnumerable<Inventario> ObtenerProductosStockCritico(
            IEnumerable<Inventario> inventarios)
        {
            if (inventarios == null)
                return Enumerable.Empty<Inventario>();

            return inventarios.Where(i => i.StockBajo);
        }

        /// <summary>
        /// Valida que una cantidad sea válida para operaciones de inventario.
        /// </summary>
        public static Result ValidarCantidad(int cantidad, int minimo = 1, int maximo = 9999)
        {
            if (cantidad < minimo)
                return Result.Failure($"La cantidad debe ser al menos {minimo}");

            if (cantidad > maximo)
                return Result.Failure($"La cantidad no puede exceder {maximo}");

            return Result.Success();
        }
    }
}
