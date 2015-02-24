using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using dialupconnmgr.Common.Extenssions;

namespace Converters
{
    public class EqualityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var param = parameter as ConverterParameter;
            if (param != null)
            {
                if (param.ValueToCompare.Equals(value))
                {
                    return param.TargetValue;
                }
                else
                {
                    return param.DefaultValue;
                }
            }
            else
            {
                return targetType.GetDefaultValue();
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
