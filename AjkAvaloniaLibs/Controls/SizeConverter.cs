using Avalonia;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Data.Converters;

namespace AjkAvaloniaLibs.Controls
{
    public class SizeConverter : IValueConverter
    {
        public static SizeConverter RowSize { get; } = new SizeConverter(1.5);
        public static SizeConverter IconSize { get; } = new SizeConverter(0.8);

        public SizeConverter(double multiply) { 
            this._multiply = multiply;
        }

        private double _multiply;
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double size)
            {
                return size * _multiply;
            }

            return 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

