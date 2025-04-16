using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AjkAvaloniaLibs.Controls
{
    public class IndentConverter : IValueConverter
    {
        public static IndentConverter Instance { get; } = new IndentConverter();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int indent)
            {
                return new Thickness(20 * indent, 0, 0, 0);
            }

            return new Thickness();
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

