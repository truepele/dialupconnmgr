using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Converters
{
    class DoubleToStringConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string result = "";
            double v = System.Convert.ToDouble(value);
            var format = parameter as string;
            if (string.IsNullOrEmpty(format))
                format = "#,#0.000";
            result = v.ToString(format, culture).Replace(culture.NumberFormat.NumberGroupSeparator, " ");

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
