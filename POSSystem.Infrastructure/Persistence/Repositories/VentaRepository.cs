using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Interfaces;
using POSSystem.Domain.ValueObjects;
using POSSystem.Infrastructure.Persistence.Context;

namespace POSSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio de ventas.
    /// </summary>
    public class VentaRepository : IVentaRepository
    {
        private readonly POSDbContext _context;

        public VentaRepository(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Venta> GetByIdAsync(Guid id)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<Venta> GetByFolioAsync(Folio folio)
        {
            if (folio == null)
                return null;

            return await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .FirstOrDefaultAsync(v => v.Folio.Valor == folio.Valor);
        }

        public async Task<Venta> GetByFolioAsync(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio))
                return null;

            return await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .FirstOrDefaultAsync(v => v.Folio.Valor == folio);
        }

        public async Task<IEnumerable<Venta>> GetPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
                .OrderByDescending(v => v.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetPorEstadoAsync(EstadoVenta estado)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .Where(v => v.Estado == estado)
                .OrderByDescending(v => v.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetVentasAbiertasAsync()
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .Where(v => v.Estado == EstadoVenta.Abierta || v.Estado == EstadoVenta.Nueva)
                .OrderByDescending(v => v.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetPorUsuarioAsync(Guid usuarioId, DateTime? fecha = null)
        {
            var query = _context.Ventas
                .Include(v => v.Detalles)
                .Include(v => v.Pagos)
                .Where(v => v.UsuarioId == usuarioId);

            if (fecha.HasValue)
            {
                var fechaInicio = fecha.Value.Date;
                var fechaFin = fechaInicio.AddDays(1);
                query = query.Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion < fechaFin);
            }

            return await query
                .OrderByDescending(v => v.FechaCreacion)
                .ToListAsync();
        }

        public async Task AddAsync(Venta venta)
        {
            if (venta == null)
                throw new ArgumentNullException(nameof(venta));

            await _context.Ventas.AddAsync(venta);
        }

        public Task UpdateAsync(Venta venta)
        {
            if (venta == null)
                throw new ArgumentNullException(nameof(venta));

            _context.Ventas.Update(venta);
            return Task.CompletedTask;
        }

        public async Task<decimal> GetTotalVentasDelDiaAsync(DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.Today;
            var fechaInicio = fechaConsulta.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await _context.Ventas
                .Where(v => v.Estado == EstadoVenta.Pagada &&
                           v.FechaCreacion >= fechaInicio &&
                           v.FechaCreacion < fechaFin)
                .SumAsync(v => v.Detalles.Sum(d => d.Total));
        }

        public async Task<Dictionary<EstadoVenta, int>> GetConteoVentasPorEstadoAsync(
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            var ventas = await _context.Ventas
                .Where(v => v.FechaCreacion >= fechaInicio && v.FechaCreacion <= fechaFin)
                .GroupBy(v => v.Estado)
                .Select(g => new { Estado = g.Key, Conteo = g.Count() })
                .ToListAsync();

            return ventas.ToDictionary(v => v.Estado, v => v.Conteo);
        }

        public async Task<bool> ExisteFolioAsync(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio))
                return false;

            return await _context.Ventas
                .AnyAsync(v => v.Folio.Valor == folio);
        }

        public async Task<IEnumerable<Venta>> GetVentasAbandonadasAsync(int minutosInactividad)
        {
            var fechaLimite = DateTime.UtcNow.AddMinutes(-minutosInactividad);

            return await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v => (v.Estado == EstadoVenta.Abierta || v.Estado == EstadoVenta.Nueva) &&
                           v.UltimaModificacion < fechaLimite)
                .ToListAsync();
        }
    }
}