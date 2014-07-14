using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.SqlServer.Server;

namespace Converters
{
    class TimeFormatConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = value;
            var format = parameter as string;
            

            if (value is DateTime)
            {
                if (string.IsNullOrEmpty(format))
                {
                    format = @"dd \.hh\:mm\:ss";
                }
                
                result = ((DateTime) value).ToString(format);
            }
            else if (value is TimeSpan)
            {
                if (string.IsNullOrEmpty(format))
                {
                    result = (int)((TimeSpan)value).TotalHours + ((TimeSpan)value).ToString(@"\:mm\:ss");
                }
                else
                {
                    result = ((TimeSpan)value).ToString(format);
                }
                
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
