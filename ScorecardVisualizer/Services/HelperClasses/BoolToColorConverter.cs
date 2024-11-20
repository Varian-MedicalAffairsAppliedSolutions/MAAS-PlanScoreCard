using System;
using System.Globalization;
using System.Windows.Data;

namespace ScorecardVisualizer.Services.HelperClasses
{
    internal class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool IsSelected)
            {
                if (IsSelected)
                    return "#DAE9F7";
            }
            return "Transparent";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
