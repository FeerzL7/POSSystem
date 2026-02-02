using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using POSSystem.Domain.DomainServices;
using POSSystem.Infrastructure.Persistence.Context;

namespace POSSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Repositorio para generación de folios únicos.
    /// Asegura que los folios sean consecutivos por día.
    /// </summary>
    public class FolioRepository : IFolioRepository
    {
        private readonly POSDbContext _context;
        private static readonly object _lock = new object();

        public FolioRepository(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Obtiene el siguiente número consecutivo para el día actual.
        /// Thread-safe mediante lock.
        /// </summary>
        public async Task<int> ObtenerSiguienteConsecutivoDelDiaAsync()
        {
            var fecha = DateTime.Now.Date;
            return await ObtenerSiguienteConsecutivoPorFechaAsync(fecha);
        }

        /// <summary>
        /// Obtiene el siguiente número consecutivo para una fecha específica.
        /// </summary>
        public async Task<int> ObtenerSiguienteConsecutivoPorFechaAsync(DateTime fecha)
        {
            lock (_lock)
            {
                var fechaStr = fecha.ToString("yyyyMMdd");

                // Buscar el último folio del día
                var ultimoFolio = _context.Ventas
                    .Where(v => v.Folio.Valor.StartsWith(fechaStr))
                    .OrderByDescending(v => v.Folio.Valor)
                    .Select(v => v.Folio.Valor)
                    .FirstOrDefault();

                if (ultimoFolio == null)
                {
                    // Primer folio del día
                    return 1;
                }

                // Extraer el consecutivo (últimos 4 dígitos)
                var partes = ultimoFolio.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int consecutivo))
                {
                    return consecutivo + 1;
                }

                // Fallback: primer folio
                return 1;
            }
        }

        /// <summary>
        /// Verifica si un folio ya existe.
        /// </summary>
        public async Task<bool> ExisteFolioAsync(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio))
                return false;

            return await _context.Ventas
                .AnyAsync(v => v.Folio.Valor == folio);
        }
    }
}