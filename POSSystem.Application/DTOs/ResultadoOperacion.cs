using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSystem.Application.DTOs
{
    /// <summary>
    /// DTO para comunicar resultados de operaciones al UI.
    /// </summary>
    public class ResultadoOperacion<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public T Datos { get; set; }
        public string CodigoError { get; set; }

        public static ResultadoOperacion<T> Exito(T datos, string mensaje = "Operación exitosa")
        {
            return new ResultadoOperacion<T>
            {
                Exitoso = true,
                Mensaje = mensaje,
                Datos = datos
            };
        }

        public static ResultadoOperacion<T> Error(string mensaje, string codigoError = null)
        {
            return new ResultadoOperacion<T>
            {
                Exitoso = false,
                Mensaje = mensaje,
                CodigoError = codigoError
            };
        }
    }

    /// <summary>
    /// Versión sin datos genéricos.
    /// </summary>
    public class ResultadoOperacion : ResultadoOperacion<object>
    {
        public new static ResultadoOperacion Exito(string mensaje = "Operación exitosa")
        {
            return new ResultadoOperacion
            {
                Exitoso = true,
                Mensaje = mensaje
            };
        }

        public new static ResultadoOperacion Error(string mensaje, string codigoError = null)
        {
            return new ResultadoOperacion
            {
                Exitoso = false,
                Mensaje = mensaje,
                CodigoError = codigoError
            };
        }
    }
}
