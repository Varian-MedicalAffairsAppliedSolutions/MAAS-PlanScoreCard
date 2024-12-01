using PlanScoreCard.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace PlanScoreCard.Converters
{
    public class StructureColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Brushes.Black;

            StructureModel structure = value as StructureModel;
           
            string templateName = structure.TemplateStructureId;
            string structureName = structure.StructureId;
            if(String.IsNullOrEmpty(structureName) && !String.IsNullOrWhiteSpace(templateName))
            {
                return Brushes.Red;
            }
            

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
