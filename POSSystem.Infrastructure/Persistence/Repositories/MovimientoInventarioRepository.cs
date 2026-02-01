using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Interfaces;
using POSSystem.Infrastructure.Persistence.Context;

namespace POSSystem.Infrastructure.Persistence.Repositories
{
    public class MovimientoInventarioRepository : IMovimientoInventarioRepository
    {
        private readonly POSDbContext _context;

        public MovimientoInventarioRepository(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MovimientoInventario> GetByIdAsync(Guid id)
        {
            return await _context.MovimientosInventario
                .Include(m => m.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<MovimientoInventario>> GetPorProductoIdAsync(
            Guid productoId,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            var query = _context.MovimientosInventario
                .Include(m => m.Producto)
                .Where(m => m.ProductoId == productoId);

            if (fechaInicio.HasValue)
                query = query.Where(m => m.FechaMovimiento >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(m => m.FechaMovimiento <= fechaFin.Value);

            return await query
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoInventario>> GetPorTipoAsync(
            TipoMovimientoInventario tipo,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null)
        {
            var query = _context.MovimientosInventario
                .Include(m => m.Producto)
                .Where(m => m.TipoMovimiento == tipo);

            if (fechaInicio.HasValue)
                query = query.Where(m => m.FechaMovimiento >= fechaInicio.Value);

            if (fechaFin.HasValue)
                query = query.Where(m => m.FechaMovimiento <= fechaFin.Value);

            return await query
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoInventario>> GetPorReferenciaAsync(string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
                return Enumerable.Empty<MovimientoInventario>();

            return await _context.MovimientosInventario
                .Include(m => m.Producto)
                .Where(m => m.Referencia == referencia)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<MovimientoInventario>> GetPorRangoFechasAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            return await _context.MovimientosInventario
                .Include(m => m.Producto)
                .Where(m => m.FechaMovimiento >= fechaInicio && m.FechaMovimiento <= fechaFin)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task AddAsync(MovimientoInventario movimiento)
        {
            if (movimiento == null)
                throw new ArgumentNullException(nameof(movimiento));

            await _context.MovimientosInventario.AddAsync(movimiento);
        }

        public async Task<IEnumerable<MovimientoInventario>> GetHistorialProductoAsync(Guid productoId)
        {
            return await _context.MovimientosInventario
                .Include(m => m.Producto)
                .Where(m => m.ProductoId == productoId)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }
    }
}