using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace POSSystem.Domain.Interfaces
{
    /// <summary>
    /// Patrón Unit of Work para gestionar transacciones.
    /// CRÍTICO: Garantiza atomicidad de operaciones.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Inicia una nueva transacción.
        /// </summary>
        /// <param name="isolationLevel">Nivel de aislamiento de la transacción</param>
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        /// <summary>
        /// Confirma la transacción actual.
        /// Persiste todos los cambios en la base de datos.
        /// </summary>
        /// <returns>Número de registros afectados</returns>
        Task<int> CommitAsync();

        /// <summary>
        /// Revierte la transacción actual.
        /// Descarta todos los cambios pendientes.
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Guarda cambios sin transacción explícita.
        /// Útil para operaciones simples.
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Repositorio de productos.
        /// </summary>
        IProductoRepository Productos { get; }

        /// <summary>
        /// Repositorio de ventas.
        /// </summary>
        IVentaRepository Ventas { get; }

        /// <summary>
        /// Repositorio de inventario.
        /// </summary>
        IInventarioRepository Inventarios { get; }

        /// <summary>
        /// Repositorio de reservas de inventario.
        /// </summary>
        IReservaInventarioRepository ReservasInventario { get; }

        /// <summary>
        /// Repositorio de cajas.
        /// </summary>
        ICajaRepository Cajas { get; }

        /// <summary>
        /// Repositorio de movimientos de inventario.
        /// </summary>
        IMovimientoInventarioRepository MovimientosInventario { get; }
    }
}
