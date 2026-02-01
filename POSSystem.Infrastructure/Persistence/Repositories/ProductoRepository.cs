using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Interfaces;
using POSSystem.Domain.ValueObjects;
using POSSystem.Infrastructure.Persistence.Context;

namespace POSSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio de productos.
    /// </summary>
    public class ProductoRepository : IProductoRepository
    {
        private readonly POSDbContext _context;

        public ProductoRepository(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Producto> GetByIdAsync(Guid id)
        {
            return await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Producto> GetByCodigoBarrasAsync(CodigoBarras codigoBarras)
        {
            if (codigoBarras == null)
                return null;

            return await _context.Productos
                .FirstOrDefaultAsync(p => p.CodigoBarras.Valor == codigoBarras.Valor);
        }

        public async Task<Producto> GetByCodigoBarrasAsync(string codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras))
                return null;

            return await _context.Productos
                .FirstOrDefaultAsync(p => p.CodigoBarras.Valor == codigoBarras);
        }

        public async Task<IEnumerable<Producto>> GetAllActivosAsync()
        {
            return await _context.Productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            return await _context.Productos
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> BuscarPorNombreAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Enumerable.Empty<Producto>();

            return await _context.Productos
                .Where(p => p.Nombre.Contains(nombre))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Producto>> GetPorCategoriaAsync(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                return Enumerable.Empty<Producto>();

            return await _context.Productos
                .Where(p => p.Categoria == categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task AddAsync(Producto producto)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            await _context.Productos.AddAsync(producto);
        }

        public Task UpdateAsync(Producto producto)
        {
            if (producto == null)
                throw new ArgumentNullException(nameof(producto));

            _context.Productos.Update(producto);
            return Task.CompletedTask;
        }

        public async Task DeleteAsync(Guid id)
        {
            var producto = await GetByIdAsync(id);
            if (producto != null)
            {
                producto.Desactivar(); // Soft delete
                _context.Productos.Update(producto);
            }
        }

        public async Task<bool> ExisteCodigoBarrasAsync(string codigoBarras)
        {
            if (string.IsNullOrWhiteSpace(codigoBarras))
                return false;

            return await _context.Productos
                .AnyAsync(p => p.CodigoBarras.Valor == codigoBarras);
        }

        public async Task<IEnumerable<Producto>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return Enumerable.Empty<Producto>();

            return await _context.Productos
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();
        }
    }
}