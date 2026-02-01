using POSSystem.Domain.Common;
using POSSystem.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Domain.DomainServices
{
    /// <summary>
    /// Servicio de dominio para generar folios únicos de venta.
    /// NOTA: Requiere acceso al repositorio para obtener el consecutivo.
    /// </summary>
    public class GeneradorFolio
    {
        private readonly IFolioRepository _folioRepository;

        public GeneradorFolio(IFolioRepository folioRepository)
        {
            _folioRepository = folioRepository ?? throw new ArgumentNullException(nameof(folioRepository));
        }

        /// <summary>
        /// Genera el siguiente folio disponible.
        /// Formato: AAAAMMDD-NNNN (ej: 20240215-0001)
        /// </summary>
        /// <returns>Nuevo folio generado</returns>
        public async Task<Result<Folio>> GenerarSiguienteFolioAsync()
        {
            try
            {
                // Obtener el siguiente consecutivo del día
                var consecutivo = await _folioRepository.ObtenerSiguienteConsecutivoDelDiaAsync();

                // Crear el folio
                var resultadoFolio = Folio.Crear(consecutivo);

                if (resultadoFolio.IsFailure)
                    return Result.Failure<Folio>(resultadoFolio.Error);

                return Result.Success(resultadoFolio.Value);
            }
            catch (Exception ex)
            {
                return Result.Failure<Folio>($"Error al generar folio: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera un folio para una fecha específica.
        /// Útil para reprocesos o ajustes.
        /// </summary>
        public async Task<Result<Folio>> GenerarFolioParaFechaAsync(DateTime fecha)
        {
            try
            {
                var consecutivo = await _folioRepository.ObtenerSiguienteConsecutivoPorFechaAsync(fecha);

                // Crear folio con fecha específica (requiere sobrecarga en Folio)
                var valor = $"{fecha:yyyyMMdd}-{consecutivo:D4}";
                var resultadoFolio = Folio.Desde(valor);

                if (resultadoFolio.IsFailure)
                    return Result.Failure<Folio>(resultadoFolio.Error);

                return Result.Success(resultadoFolio.Value);
            }
            catch (Exception ex)
            {
                return Result.Failure<Folio>($"Error al generar folio para fecha: {ex.Message}");
            }
        }

        /// <summary>
        /// Valida que un folio no exista ya en el sistema.
        /// </summary>
        public async Task<bool> ExisteFolioAsync(Folio folio)
        {
            if (folio == null)
                return false;

            return await _folioRepository.ExisteFolioAsync(folio.Valor);
        }
    }

    /// <summary>
    /// Interface para el repositorio de folios.
    /// Se implementará en Infrastructure.
    /// </summary>
    public interface IFolioRepository
    {
        /// <summary>
        /// Obtiene el siguiente número consecutivo para el día actual.
        /// </summary>
        Task<int> ObtenerSiguienteConsecutivoDelDiaAsync();

        /// <summary>
        /// Obtiene el siguiente número consecutivo para una fecha específica.
        /// </summary>
        Task<int> ObtenerSiguienteConsecutivoPorFechaAsync(DateTime fecha);

        /// <summary>
        /// Verifica si un folio ya existe en el sistema.
        /// </summary>
        Task<bool> ExisteFolioAsync(string folio);
    }
}
