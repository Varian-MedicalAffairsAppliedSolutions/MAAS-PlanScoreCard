using PlanScoreCard.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class StructureToStructureDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            StructureModel structure = value as StructureModel;

            string templateName = structure.TemplateStructureId;
            string structureName = structure.StructureId;

            if (String.IsNullOrEmpty(structureName))
            {
                return templateName;
            }
            else if (!templateName.Equals(structureName))
            {
                return templateName + "(" + structureName + ")";
            }

            return structureName;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
