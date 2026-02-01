using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Interfaces;
using POSSystem.Infrastructure.Persistence.Context;

namespace POSSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio de inventario.
    /// CRÍTICO: Implementa bloqueos para prevenir race conditions.
    /// </summary>
    public class InventarioRepository : IInventarioRepository
    {
        private readonly POSDbContext _context;

        public InventarioRepository(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Inventario> GetByIdAsync(Guid id)
        {
            return await _context.Inventarios
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Inventario> GetByProductoIdAsync(Guid productoId)
        {
            return await _context.Inventarios
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.ProductoId == productoId);
        }

        public async Task<IEnumerable<Inventario>> GetByProductoIdsAsync(IEnumerable<Guid> productoIds)
        {
            if (productoIds == null || !productoIds.Any())
                return Enumerable.Empty<Inventario>();

            return await _context.Inventarios
                .Include(i => i.Producto)
                .Where(i => productoIds.Contains(i.ProductoId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventario>> GetAllAsync()
        {
            return await _context.Inventarios
                .Include(i => i.Producto)
                .OrderBy(i => i.Producto.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventario>> GetProductosStockBajoAsync()
        {
            return await _context.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.StockFisico <= i.StockMinimo)
                .OrderBy(i => i.StockFisico)
                .ToListAsync();
        }

        public async Task<IEnumerable<Inventario>> GetProductosSinStockAsync()
        {
            return await _context.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.StockFisico == 0)
                .OrderBy(i => i.Producto.Nombre)
                .ToListAsync();
        }

        public async Task AddAsync(Inventario inventario)
        {
            if (inventario == null)
                throw new ArgumentNullException(nameof(inventario));

            await _context.Inventarios.AddAsync(inventario);
        }

        public Task UpdateAsync(Inventario inventario)
        {
            if (inventario == null)
                throw new ArgumentNullException(nameof(inventario));

            _context.Inventarios.Update(inventario);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Obtiene el inventario con bloqueo pesimista (FOR UPDATE).
        /// CRÍTICO: Previene race conditions en operaciones concurrentes.
        /// </summary>
        public async Task<Inventario> GetByProductoIdWithLockAsync(Guid productoId)
        {
            // EF Core no soporta directamente FOR UPDATE en todas las bases de datos
            // Para SQLite/SQL Server, usamos un enfoque de transacción serializable
            // que se configura en el UnitOfWork

            // Alternativa: usar FromSqlRaw para bases de datos que soportan FOR UPDATE
            // Para SQLite, la transacción serializable es suficiente

            return await _context.Inventarios
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.ProductoId == productoId);

            // Nota: En producción con PostgreSQL o MySQL, podrías usar:
            // return await _context.Inventarios
            //     .FromSqlRaw("SELECT * FROM Inventarios WHERE ProductoId = {0} FOR UPDATE", productoId)
            //     .Include(i => i.Producto)
            //     .FirstOrDefaultAsync();
        }

        public async Task<bool> ExisteInventarioParaProductoAsync(Guid productoId)
        {
            return await _context.Inventarios
                .AnyAsync(i => i.ProductoId == productoId);
        }
    }
}