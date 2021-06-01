using System;
using System.Globalization;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class DoubleF2Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                double val = 0.0;
                if (Double.TryParse(value.ToString(), out val))
                {
                    if (Double.IsNaN(val))
                    {
                        return " - ";
                    }
                    else if (val < -999)
                    {
                        return " - ";
                    }
                    return val.ToString("F2");
                }
            }
            return " - ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
