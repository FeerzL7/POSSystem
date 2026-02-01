using POSSystem.Application.DTOs;
using POSSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.Mappers
{
    public static class CajaMapper
    {
        public static CajaDto ToDto(this Caja caja)
        {
            if (caja == null) return null;

            return new CajaDto
            {
                Id = caja.Id,
                NumeroCaja = caja.NumeroCaja,
                Nombre = caja.Nombre,
                EstaAbierta = caja.EstaAbierta,
                FondoInicial = caja.FondoInicial,
                SaldoActual = caja.SaldoActual,
                SaldoCalculado = caja.SaldoCalculado,
                FechaApertura = caja.FechaApertura,
                FechaCierre = caja.FechaCierre,
                TotalVentas = caja.TotalVentas,
                TotalCancelaciones = caja.TotalCancelaciones,
                TotalRetiros = caja.TotalRetiros,
                Diferencia = caja.Diferencia,
                ObservacionesCierre = caja.ObservacionesCierre
            };
        }
    }
}
