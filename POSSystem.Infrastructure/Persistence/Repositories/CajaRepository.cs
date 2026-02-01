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
    public class CajaRepository : ICajaRepository
    {
        private readonly POSDbContext _context;

        public CajaRepository(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Caja> GetByIdAsync(Guid id)
        {
            return await _context.Cajas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Caja> GetByNumeroAsync(int numeroCaja)
        {
            return await _context.Cajas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.NumeroCaja == numeroCaja);
        }

        public async Task<Caja> GetCajaAbiertaAsync()
        {
            return await _context.Cajas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.EstaAbierta);
        }

        public async Task<IEnumerable<Caja>> GetAllAsync()
        {
            return await _context.Cajas
                .OrderBy(c => c.NumeroCaja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Caja>> GetPorEstadoAsync(bool estaAbierta)
        {
            return await _context.Cajas
                .Where(c => c.EstaAbierta == estaAbierta)
                .OrderBy(c => c.NumeroCaja)
                .ToListAsync();
        }

        public async Task<IEnumerable<Caja>> GetHistorialCierresAsync(
            int numeroCaja,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            return await _context.Cajas
                .Include(c => c.Movimientos)
                .Where(c => c.NumeroCaja == numeroCaja &&
                           c.FechaCierre.HasValue &&
                           c.FechaCierre >= fechaInicio &&
                           c.FechaCierre <= fechaFin)
                .OrderByDescending(c => c.FechaCierre)
                .ToListAsync();
        }

        public async Task AddAsync(Caja caja)
        {
            if (caja == null)
                throw new ArgumentNullException(nameof(caja));

            await _context.Cajas.AddAsync(caja);
        }

        public Task UpdateAsync(Caja caja)
        {
            if (caja == null)
                throw new ArgumentNullException(nameof(caja));

            _context.Cajas.Update(caja);
            return Task.CompletedTask;
        }

        public async Task<bool> ExisteCajaAbiertaAsync()
        {
            return await _context.Cajas.AnyAsync(c => c.EstaAbierta);
        }

        public async Task<decimal> GetTotalVentasCajaAsync(
            Guid cajaId,
            DateTime fechaInicio,
            DateTime fechaFin)
        {
            return await _context.MovimientosCaja
                .Where(m => m.CajaId == cajaId &&
                           m.TipoMovimiento == Domain.Enums.TipoMovimientoCaja.Venta &&
                           m.FechaMovimiento >= fechaInicio &&
                           m.FechaMovimiento <= fechaFin)
                .SumAsync(m => m.Monto);
        }
    }
}