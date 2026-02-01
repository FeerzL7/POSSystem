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
    public class ReservaInventarioRepository : IReservaInventarioRepository
    {
        private readonly POSDbContext _context;

        public ReservaInventarioRepository(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ReservaInventario> GetByIdAsync(Guid id)
        {
            return await _context.ReservasInventario
                .Include(r => r.Producto)
                .Include(r => r.Venta)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<ReservaInventario>> GetPorVentaIdAsync(Guid ventaId)
        {
            return await _context.ReservasInventario
                .Include(r => r.Producto)
                .Where(r => r.VentaId == ventaId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservaInventario>> GetReservasActivasPorProductoAsync(Guid productoId)
        {
            return await _context.ReservasInventario
                .Where(r => r.ProductoId == productoId && r.Estado == EstadoReserva.Activa)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservaInventario>> GetReservasActivasAsync()
        {
            return await _context.ReservasInventario
                .Include(r => r.Producto)
                .Where(r => r.Estado == EstadoReserva.Activa)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservaInventario>> GetReservasExpiradasAsync()
        {
            var ahora = DateTime.UtcNow;

            return await _context.ReservasInventario
                .Include(r => r.Producto)
                .Where(r => r.Estado == EstadoReserva.Activa && r.FechaExpiracion < ahora)
                .ToListAsync();
        }

        public async Task<IEnumerable<ReservaInventario>> GetPorEstadoAsync(EstadoReserva estado)
        {
            return await _context.ReservasInventario
                .Include(r => r.Producto)
                .Where(r => r.Estado == estado)
                .OrderByDescending(r => r.FechaCreacion)
                .ToListAsync();
        }

        public async Task AddAsync(ReservaInventario reserva)
        {
            if (reserva == null)
                throw new ArgumentNullException(nameof(reserva));

            await _context.ReservasInventario.AddAsync(reserva);
        }

        public Task UpdateAsync(ReservaInventario reserva)
        {
            if (reserva == null)
                throw new ArgumentNullException(nameof(reserva));

            _context.ReservasInventario.Update(reserva);
            return Task.CompletedTask;
        }

        public async Task<int> GetCantidadReservadaPorProductoAsync(Guid productoId)
        {
            return await _context.ReservasInventario
                .Where(r => r.ProductoId == productoId && r.Estado == EstadoReserva.Activa)
                .SumAsync(r => r.Cantidad);
        }

        public async Task EliminarReservasAntiguasAsync(DateTime fechaLimite)
        {
            var reservasAntiguas = await _context.ReservasInventario
                .Where(r => r.Estado != EstadoReserva.Activa && r.FechaCreacion < fechaLimite)
                .ToListAsync();

            _context.ReservasInventario.RemoveRange(reservasAntiguas);
        }
    }
}