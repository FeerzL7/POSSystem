using System;
using System.Globalization;
using System.Windows.Data;

namespace POSSystem.Presentation.WPF.Converters
{
    public class BoolToEstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool estado)
            {
                return estado ? "ABIERTA" : "CERRADA";
            }
            return "CERRADA";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}