using PlanScoreCard.Models;
using PlanScoreCard.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class MetricEditorConverter : IValueConverter
    {

        private ViewLauncherService ViewLauncherService;

        public MetricEditorConverter(ViewLauncherService viewLauncherService)
        {
            ViewLauncherService = viewLauncherService;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as ScoreMetricModel == null)
                return null;

            ScoreMetricModel scoreMetric = value as ScoreMetricModel;

            if (scoreMetric.MetricType == MetricTypeEnum.ConformationNumber)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.ConformityIndex)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.DoseAtVolume)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.MaxDose)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.MeanDose)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.MinDose)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.Volume)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.VolumeAtDose)
            {

            }
            else if (scoreMetric.MetricType == MetricTypeEnum.VolumeOfRegret)
            {

            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
