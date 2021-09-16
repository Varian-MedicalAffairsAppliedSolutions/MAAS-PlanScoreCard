using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class DoubleDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string valueString = (string)value;

            string valueString = String.Format("{0:n}", value);

            return valueString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //double val = 0.0;
            if (value.ToString().Count(x => x == '.') > 1)
            {
                return System.Convert.ToDouble(value.ToString().Substring(0, GetSecondDot(value.ToString())));
            }
            return value;
        }
        internal int GetSecondDot(string val)
        {
            int count = 0;
            for (int i = 0; i < val.Length; i++)
            {
                if (val[i] == '.')
                {
                    count++;
                    if (count == 2)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
