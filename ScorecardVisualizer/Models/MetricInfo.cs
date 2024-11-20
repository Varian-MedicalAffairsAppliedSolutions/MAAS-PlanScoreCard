using OxyPlot;
using PlanScoreCard.API.Models.ScoreCard;
using ScorecardVisualizer.Services;
using System.Linq;

namespace ScorecardVisualizer.Models
{
    public class MetricInfo
    {
        public string MetricDisplay { get; set; }
        public double Points { get; set; }
        public double FractionOfTotal { get; set; }
        public OxyColor Color { get; set; }


        public MetricInfo(ScoreTemplateModel scoreTemplate, double totalScore, OxyColor color)
        {
            string inputUnit = scoreTemplate.InputUnit;
            double inputValue = scoreTemplate.InputValue;
            string metricType = scoreTemplate.MetricType;
            string outputUnit = scoreTemplate.OutputUnit;

            Points = scoreTemplate.ScorePoints.Select(s => s.Score).Max();

            if (metricType == "DoseAtVolume" || metricType == "VolumeAtDose")
                MetricDisplay = $"{Dictionaries.MetricPrefix[metricType]}{inputValue}{inputUnit} [{outputUnit}]";
            else
                MetricDisplay = Dictionaries.MetricPrefix[metricType];

            FractionOfTotal = Points / totalScore;
            Color = color;
        }
    }
}
