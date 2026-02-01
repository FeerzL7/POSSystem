using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using POSSystem.Application.DTOs;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Caja
{
    /// <summary>
    /// Caso de uso: Registrar retiro de efectivo de la caja.
    /// </summary>
    public class RegistrarRetiroEfectivoUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegistrarRetiroEfectivoUseCase> _logger;

        public RegistrarRetiroEfectivoUseCase(
            IUnitOfWork unitOfWork,
            ILogger<RegistrarRetiroEfectivoUseCase> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResultadoOperacion> ExecuteAsync(
            decimal monto,
            string motivo,
            Guid usuarioId)
        {
            try
            {
                if (monto <= 0)
                {
                    return ResultadoOperacion.Error(
                        "El monto debe ser mayor a cero",
                        "MONTO_INVALIDO");
                }

                if (string.IsNullOrWhiteSpace(motivo))
                {
                    return ResultadoOperacion.Error(
                        "Debe proporcionar un motivo para el retiro",
                        "MOTIVO_REQUERIDO");
                }

                var caja = await _unitOfWork.Cajas.GetCajaAbiertaAsync();
                if (caja == null)
                {
                    return ResultadoOperacion.Error(
                        "No hay caja abierta",
                        "CAJA_NO_ABIERTA");
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var resultado = caja.RegistrarRetiro(monto, motivo, usuarioId);
                    if (resultado.IsFailure)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ResultadoOperacion.Error(resultado.Error);
                    }

                    await _unitOfWork.Cajas.UpdateAsync(caja);
                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Retiro de {Monto:C} registrado en caja. Motivo: {Motivo}",
                        monto, motivo);

                    return ResultadoOperacion.Exito(
                        $"Retiro de {monto:C} registrado exitosamente");
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registrando retiro de efectivo");
                return ResultadoOperacion.Error(
                    $"Error al registrar retiro: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}