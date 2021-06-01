using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models.Internals
{
    public class ScorePointInternalModel
    {
        public ScorePointInternalModel(double pointX, double score, bool variation, PlanScoreColorModel planScoreColor)
        {
            PointX = pointX;
            Score = score;
            Variation = variation;
            Colors = planScoreColor != null ? planScoreColor.Colors : new List<double>();
            Label = planScoreColor != null ? planScoreColor.ColorLabel : string.Empty;
            ColorValue = planScoreColor != null ? planScoreColor.ColorValue : 0.0;
        }

        public double PointX { get; private set; }
        public double Score { get; set; }
        public bool Variation { get; set; }
        public List<double> Colors { get; set; }
        public string Label { get; set; }
        public double ColorValue { get; }
    }
}
