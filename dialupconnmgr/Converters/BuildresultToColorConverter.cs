using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Converters
{
    public sealed class BuildresultToColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color result = Colors.Gray;
            bool? val = value as bool?;
            if (val != null)
            {
                if (val == true) 
                {
                    result = Colors.Green;
                }
                else
                {
                    result = Colors.Red;
                }
            }

            return new SolidColorBrush(result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
