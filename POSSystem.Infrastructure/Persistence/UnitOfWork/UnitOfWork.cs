using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using POSSystem.Domain.Exceptions;
using POSSystem.Domain.Interfaces;
using POSSystem.Infrastructure.Persistence.Context;
using POSSystem.Infrastructure.Persistence.Repositories;

namespace POSSystem.Infrastructure.Persistence.UnitOfWork
{
    /// <summary>
    /// Implementación del patrón Unit of Work.
    /// CRÍTICO: Garantiza atomicidad de operaciones mediante transacciones.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly POSDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _transaction;

        // Repositorios lazy-loaded
        private IProductoRepository _productos;
        private IVentaRepository _ventas;
        private IInventarioRepository _inventarios;
        private IReservaInventarioRepository _reservasInventario;
        private ICajaRepository _cajas;
        private IMovimientoInventarioRepository _movimientosInventario;

        public UnitOfWork(
            POSDbContext context,
            ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Propiedades de repositorios (lazy loading)
        public IProductoRepository Productos =>
            _productos ??= new ProductoRepository(_context);

        public IVentaRepository Ventas =>
            _ventas ??= new VentaRepository(_context);

        public IInventarioRepository Inventarios =>
            _inventarios ??= new InventarioRepository(_context);

        public IReservaInventarioRepository ReservasInventario =>
            _reservasInventario ??= new ReservaInventarioRepository(_context);

        public ICajaRepository Cajas =>
            _cajas ??= new CajaRepository(_context);

        public IMovimientoInventarioRepository MovimientosInventario =>
            _movimientosInventario ??= new MovimientoInventarioRepository(_context);

        /// <summary>
        /// Inicia una nueva transacción.
        /// </summary>
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
            {
                _logger.LogWarning("Intento de iniciar transacción cuando ya hay una activa");
                throw new InvalidOperationException("Ya hay una transacción activa");
            }

            _transaction = await _context.Database.BeginTransactionAsync(isolationLevel);

            _logger.LogDebug(
                "Transacción iniciada con nivel de aislamiento: {IsolationLevel}",
                isolationLevel);
        }

        /// <summary>
        /// Confirma la transacción actual y persiste los cambios.
        /// </summary>
        public async Task<int> CommitAsync()
        {
            try
            {
                // Guardar cambios en la base de datos
                var result = await _context.SaveChangesAsync();

                // Confirmar transacción si existe
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    _logger.LogDebug("Transacción confirmada. Registros afectados: {Count}", result);

                    // Limpiar la transacción
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }

                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al confirmar transacción");
                await RollbackAsync();
                throw new ConcurrencyException(
                    "Los datos fueron modificados por otro usuario. Por favor, recargue e intente nuevamente.",
                    ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al confirmar transacción");
                await RollbackAsync();
                throw new InvalidOperationException(
                    "Error al guardar los cambios en la base de datos. Verifique los datos e intente nuevamente.",
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al confirmar transacción");
                await RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Revierte la transacción actual.
        /// </summary>
        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                    _logger.LogWarning("Transacción revertida");

                    await _transaction.DisposeAsync();
                    _transaction = null;
                }

                // Descartar cambios no guardados en el contexto
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            entry.State = EntityState.Detached;
                            break;
                        case EntityState.Modified:
                        case EntityState.Deleted:
                            entry.Reload();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al revertir transacción");
                throw;
            }
        }

        /// <summary>
        /// Guarda cambios sin transacción explícita.
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Error de concurrencia al guardar cambios");
                throw new ConcurrencyException(
                    "Los datos fueron modificados por otro usuario.",
                    ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar cambios");
                throw;
            }
        }

        /// <summary>
        /// Libera recursos.
        /// </summary>
        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}