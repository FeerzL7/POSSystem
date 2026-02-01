using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Caja
{
    /// <summary>
    /// Caso de uso: Cerrar caja al finalizar operaciones.
    /// Registra diferencias y genera reporte de cierre.
    /// </summary>
    public class CerrarCajaUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CerrarCajaUseCase> _logger;

        public CerrarCajaUseCase(
            IUnitOfWork unitOfWork,
            ILogger<CerrarCajaUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResultadoOperacion<CajaDto>> ExecuteAsync(
            decimal saldoDeclarado,
            Guid usuarioId,
            string observaciones = null)
        {
            try
            {
                _logger.LogInformation(
                    "Cerrando caja. Saldo declarado: {SaldoDeclarado:C}",
                    saldoDeclarado);

                // Obtener caja abierta
                var caja = await _unitOfWork.Cajas.GetCajaAbiertaAsync();
                if (caja == null)
                {
                    return ResultadoOperacion<CajaDto>.Error(
                        "No hay ninguna caja abierta",
                        "CAJA_NO_ABIERTA");
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Cerrar la caja
                    var resultadoCerrar = caja.Cerrar(saldoDeclarado, usuarioId, observaciones);
                    if (resultadoCerrar.IsFailure)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion<CajaDto>.Error(resultadoCerrar.Error);
                    }

                    await _unitOfWork.Cajas.UpdateAsync(caja);
                    await _unitOfWork.CommitAsync();

                    // Log de diferencias
                    if (caja.Diferencia.HasValue && caja.Diferencia.Value != 0)
                    {
                        _logger.LogWarning(
                            "Caja {NumeroCaja} cerrada con diferencia: {Diferencia:C}",
                            caja.NumeroCaja, caja.Diferencia.Value);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Caja {NumeroCaja} cerrada sin diferencias",
                            caja.NumeroCaja);
                    }

                    var cajaDto = caja.ToDto();
                    return ResultadoOperacion<CajaDto>.Exito(
                        cajaDto,
                        $"Caja {caja.NumeroCaja} cerrada exitosamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en transacción de cierre de caja");
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico cerrando caja");
                return ResultadoOperacion<CajaDto>.Error(
                    $"Error al cerrar caja: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}