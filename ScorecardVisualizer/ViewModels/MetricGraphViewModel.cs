using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.API.Models.ScoreCard;
using ScorecardVisualizer.Services;
using System.Collections.Generic;
using System.Linq;

namespace ScorecardVisualizer.ViewModels
{
    internal class MetricGraphViewModel : ViewModelBase
    {
        public PlotModel Plot { get; set; }


        public MetricGraphViewModel(ScoreTemplateModel scoreTemplateModel)
        {
            //Name = scoreTemplateModel.ScorePoints.Count().ToString();

            string plotTitle, xAxisTitle, yAxisTitle;

            if (scoreTemplateModel.MetricType == "DoseAtVolume")
            {
                plotTitle = $"{Dictionaries.MetricPrefix[scoreTemplateModel.MetricType]}{scoreTemplateModel.InputValue}{scoreTemplateModel.InputUnit} [{scoreTemplateModel.OutputUnit}]";
                xAxisTitle = "Dose [Gy]";
                yAxisTitle = "Score";
            }
            else if (scoreTemplateModel.MetricType == "VolumeAtDose")
            {
                plotTitle = $"{Dictionaries.MetricPrefix[scoreTemplateModel.MetricType]}{scoreTemplateModel.InputValue}{scoreTemplateModel.InputUnit} [{scoreTemplateModel.OutputUnit}]";
                xAxisTitle = "Volume [cc]";
                yAxisTitle = "Score";
            }
            else
            {
                plotTitle = Dictionaries.MetricPrefix[scoreTemplateModel.MetricType];
                xAxisTitle = "Value";
                yAxisTitle = "Score";
            }

            Plot = new PlotModel();

            LineSeries lineSeries = new LineSeries();

            Plot.Title = plotTitle;
            Plot.TitleFontSize = 14;

            Plot.Axes.Add(new LinearAxis
            {
                Title = xAxisTitle,
                Position = AxisPosition.Bottom
            });

            Plot.Axes.Add(new LinearAxis
            {
                Title = yAxisTitle,
                Position = AxisPosition.Left
            });

            List<DataPoint> allPoints = new List<DataPoint>();

            foreach (var item in scoreTemplateModel.ScorePoints)
            {
                allPoints.Add(new DataPoint(item.PointX, item.Score));
            }

            lineSeries.Points.AddRange(allPoints.OrderBy(p => p.X));

            Plot.Series.Add(lineSeries);

        }
    }
}
