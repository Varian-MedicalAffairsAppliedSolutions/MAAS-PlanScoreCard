using System;
using System.Globalization;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class MetricIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int metricId = -1;
            if (int.TryParse(value.ToString(), out metricId))
            {
                return metricId + 1;
            }
            return metricId;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
