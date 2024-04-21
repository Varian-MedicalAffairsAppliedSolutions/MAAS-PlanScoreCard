using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class MetricIdToRankConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int myVal = 0;
            if(int.TryParse(value.ToString(),out myVal))
            {
                return myVal + 1;//add 1 to the number to make is 1 indexed
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
