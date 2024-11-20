using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ScorecardVisualizer.Services.HelperClasses
{
    internal class OxyColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OxyPlot.OxyColor oxyColor)
                return new SolidColorBrush(Color.FromRgb(oxyColor.R, oxyColor.G, oxyColor.B));
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
