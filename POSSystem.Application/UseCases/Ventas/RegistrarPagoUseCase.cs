using System;
using System.Threading.Tasks;
using POSSystem.Application.DTOs;
using POSSystem.Domain.Enums;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Ventas
{
    /// <summary>
    /// Caso de uso: Registrar un pago en una venta.
    /// NO finaliza la venta, solo registra el pago.
    /// </summary>
    public class RegistrarPagoUseCase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RegistrarPagoUseCase(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ResultadoOperacion<PagoDto>> ExecuteAsync(
            Guid ventaId,
            decimal montoPagado,
            TipoPago tipoPago,
            string referencia = null)
        {
            try
            {
                // NOTA: En la implementación real con MVVM, la venta está en memoria
                // Aquí simularíamos el registro del pago
                // La venta se guarda en FinalizarVentaUseCase

                if (montoPagado <= 0)
                {
                    return ResultadoOperacion<PagoDto>.Error(
                        "El monto pagado debe ser mayor a cero",
                        "MONTO_INVALIDO");
                }

                // Validar referencia para pagos electrónicos
                if ((tipoPago == TipoPago.TarjetaCredito ||
                     tipoPago == TipoPago.TarjetaDebito ||
                     tipoPago == TipoPago.Transferencia) &&
                    string.IsNullOrWhiteSpace(referencia))
                {
                    return ResultadoOperacion<PagoDto>.Error(
                        "Se requiere número de referencia para pagos electrónicos",
                        "REFERENCIA_REQUERIDA");
                }

                // El pago real se registra en la entidad Venta en memoria
                // y se persiste en FinalizarVentaUseCase

                var pagoDto = new PagoDto
                {
                    Id = Guid.NewGuid(),
                    Monto = montoPagado,
                    TipoPago = tipoPago,
                    FechaPago = DateTime.UtcNow,
                    Referencia = referencia ?? string.Empty,
                    Cambio = 0 // Se calcula en la entidad
                };

                return ResultadoOperacion<PagoDto>.Exito(
                    pagoDto,
                    "Pago registrado correctamente");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<PagoDto>.Error(
                    $"Error al registrar pago: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}