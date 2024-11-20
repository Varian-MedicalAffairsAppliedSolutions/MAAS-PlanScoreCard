using System;
using System.Globalization;
using System.Windows.Data;

namespace ScorecardVisualizer.Services.HelperClasses
{
    internal class BorderSizeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is double outerWidth && values[1] is double fraction)
            {
                return outerWidth * fraction;
            }
            return Binding.DoNothing;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
