using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class MaxScoreToHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "Score";

            if (value.ToString() == "0")
                return "Score";

            string outputString = Math.Round((double)value, 2).ToString();
            return "Score (X/" + outputString + ")";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
