using PlanScoreCard.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class StructureToBoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return FontWeights.Normal;

            StructureModel structure = value as StructureModel;

            string templateName = structure.TemplateStructureId;
            string structureName = structure.StructureId;

            if (String.IsNullOrWhiteSpace(templateName))
                return FontWeights.Normal;

            if (!templateName.Equals(structureName))
            {
                return FontWeights.Bold;
            }

            return FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
