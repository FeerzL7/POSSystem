using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Caja
{
    /// <summary>
    /// Caso de uso: Abrir caja para iniciar operaciones.
    /// </summary>
    public class AbrirCajaUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AbrirCajaUseCase> _logger;

        public AbrirCajaUseCase(
            IUnitOfWork unitOfWork,
            ILogger<AbrirCajaUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResultadoOperacion<CajaDto>> ExecuteAsync(
            int numeroCaja,
            decimal fondoInicial,
            Guid usuarioId)
        {
            try
            {
                _logger.LogInformation(
                    "Abriendo caja {NumeroCaja}. Fondo inicial: {FondoInicial:C}",
                    numeroCaja, fondoInicial);

                // Validar que no haya otra caja abierta
                var cajaAbierta = await _unitOfWork.Cajas.GetCajaAbiertaAsync();
                if (cajaAbierta != null)
                {
                    return ResultadoOperacion<CajaDto>.Error(
                        $"Ya hay una caja abierta: Caja {cajaAbierta.NumeroCaja}",
                        "CAJA_YA_ABIERTA");
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Buscar o crear la caja
                    var caja = await _unitOfWork.Cajas.GetByNumeroAsync(numeroCaja);

                    if (caja == null)
                    {
                        // Crear nueva caja
                        var resultadoCrear = Domain.Entities.Caja.Crear(
                            numeroCaja,
                            $"Caja {numeroCaja}");

                        if (resultadoCrear.IsFailure)
                        {
                            await _unitOfWork.RollbackAsync();
                            return ResultadoOperacion<CajaDto>.Error(resultadoCrear.Error);
                        }

                        caja = resultadoCrear.Value;
                        await _unitOfWork.Cajas.AddAsync(caja);
                    }

                    // Abrir la caja
                    var resultadoAbrir = caja.Abrir(fondoInicial, usuarioId);
                    if (resultadoAbrir.IsFailure)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion<CajaDto>.Error(resultadoAbrir.Error);
                    }

                    await _unitOfWork.Cajas.UpdateAsync(caja);
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Caja {NumeroCaja} abierta exitosamente",
                        numeroCaja);

                    var cajaDto = caja.ToDto();
                    return ResultadoOperacion<CajaDto>.Exito(
                        cajaDto,
                        $"Caja {numeroCaja} abierta exitosamente");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en transacción de apertura de caja");
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico abriendo caja");
                return ResultadoOperacion<CajaDto>.Error(
                    $"Error al abrir caja: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}