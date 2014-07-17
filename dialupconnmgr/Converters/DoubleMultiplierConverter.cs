using System;
using System.Windows.Data;

namespace Converters
{
    class DoubleMultiplierConverter : IValueConverter
    {
        private const double DEFAULT_MUL = 1.0/(1024*1024);
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double mul;

            if(!double.TryParse((parameter as string), out mul))
                mul = DEFAULT_MUL;

            return System.Convert.ToDouble(value)*mul;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
