using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PlanScoreCard.Models.Internals
{
    public class PlanScoreColorModel : BindableBase
    {

        private System.Windows.Media.Brush planScoreBackgroundColor;

        public System.Windows.Media.Brush PlanScoreBackgroundColor
        {
            get { return planScoreBackgroundColor; }
            set { SetProperty(ref planScoreBackgroundColor , value); }
        }

        public string ColorLabel { get; set; }
        public double ColorValue { get; set; }
        public List<double> Colors { get; set; }
        public PlanScoreColorModel(List<double> colors, string label)
        {
            Colors = colors;
            ColorLabel = label;
            double colorValue = 0.0;
            var provider = new CultureInfo("en-US");
            if (Double.TryParse(label.Split('[').Last().TrimEnd(']'), NumberStyles.Float, provider, out colorValue))
            {
                ColorValue = colorValue;
            }
            PlanScoreBackgroundColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(Convert.ToByte(colors.ElementAt(0)),
                Convert.ToByte(colors.ElementAt(1)),
                Convert.ToByte(colors.ElementAt(2))));
        }
    }
}
