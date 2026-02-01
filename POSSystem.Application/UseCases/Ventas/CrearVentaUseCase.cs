using System;
using System.Threading.Tasks;
using POSSystem.Application.DTOs;
using POSSystem.Application.Mappers;
using POSSystem.Domain.DomainServices;
using POSSystem.Domain.Entities;
using POSSystem.Domain.Interfaces;

namespace POSSystem.Application.UseCases.Ventas
{
    /// <summary>
    /// Caso de uso: Crear una nueva venta.
    /// </summary>
    public class CrearVentaUseCase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GeneradorFolio _generadorFolio;

        public CrearVentaUseCase(IUnitOfWork unitOfWork, GeneradorFolio generadorFolio)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _generadorFolio = generadorFolio ?? throw new ArgumentNullException(nameof(generadorFolio));
        }

        public async Task<ResultadoOperacion<VentaDto>> ExecuteAsync(Guid usuarioId)
        {
            try
            {
                // 1. Validar que haya una caja abierta
                var cajaAbierta = await _unitOfWork.Cajas.GetCajaAbiertaAsync();
                if (cajaAbierta == null)
                {
                    return ResultadoOperacion<VentaDto>.Error(
                        "No hay ninguna caja abierta. Abra una caja antes de realizar ventas.",
                        "CAJA_NO_ABIERTA");
                }

                // 2. Generar folio único
                var resultadoFolio = await _generadorFolio.GenerarSiguienteFolioAsync();
                if (resultadoFolio.IsFailure)
                {
                    return ResultadoOperacion<VentaDto>.Error(
                        resultadoFolio.Error,
                        "ERROR_GENERAR_FOLIO");
                }

                // 3. Crear la venta
                var resultadoVenta = Venta.Crear(resultadoFolio.Value, usuarioId);
                if (resultadoVenta.IsFailure)
                {
                    return ResultadoOperacion<VentaDto>.Error(resultadoVenta.Error);
                }

                var venta = resultadoVenta.Value;

                // 4. Persistir (solo en memoria por ahora, se guarda al finalizar)
                // Para este caso, NO guardamos en BD hasta finalizar la venta
                // Pero retornamos el DTO para que la UI pueda trabajar con ella

                var ventaDto = venta.ToDto();
                return ResultadoOperacion<VentaDto>.Exito(
                    ventaDto,
                    $"Nueva venta creada: {venta.Folio.Valor}");
            }
            catch (Exception ex)
            {
                return ResultadoOperacion<VentaDto>.Error(
                    $"Error al crear venta: {ex.Message}",
                    "ERROR_SISTEMA");
            }
        }
    }
}