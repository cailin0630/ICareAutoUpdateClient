using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace ICareAutoUpdateClient.Converter
{

    [ValueConversion(typeof(double), typeof(double))]
    public class PercentageToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ret = double.Parse(value?.ToString() ?? "0") * (410.0 / 100);
            Console.WriteLine(ret);
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

}
