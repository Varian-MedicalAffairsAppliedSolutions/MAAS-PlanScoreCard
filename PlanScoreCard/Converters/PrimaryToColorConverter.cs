using System;
using System.Globalization;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class PrimaryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return System.Windows.Media.Brushes.CornflowerBlue;
            }
            return System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
