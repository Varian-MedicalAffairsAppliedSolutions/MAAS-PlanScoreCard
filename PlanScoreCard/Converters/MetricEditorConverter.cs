using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
using PlanScoreCard.Views.MetricEditors;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace PlanScoreCard.Converters
{
    public class MetricEditorConverter : IValueConverter
    {
        
        private ViewLauncherService ViewLauncherService;
        private IEventAggregator EventAggregator;

        public MetricEditorConverter(ViewLauncherService viewLauncherService, IEventAggregator eventAggregator)
        {
            ViewLauncherService = viewLauncherService;
            EventAggregator = eventAggregator; 
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as ScoreMetricModel == null)
                return null;

            ScoreMetricModel scoreMetric = value as ScoreMetricModel;

            if (scoreMetric.MetricType == MetricTypeEnum.ConformityIndex || scoreMetric.MetricType == MetricTypeEnum.ConformationNumber)
            {
                EditCIView volumeAtDoseView = ViewLauncherService.GetEditMetricView_CI();
                EventAggregator.GetEvent<ShowCIMetricEvent>().Publish(scoreMetric);
                return volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.DoseAtVolume)
            {
                EditDoseAtVolumeView volumeAtDoseView = ViewLauncherService.GetEditMetricView_DoseAtVolume();
                EventAggregator.GetEvent<ShowDoseAtVolumeMetricEvent>().Publish(scoreMetric);
                return volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
            {
                EditHIView volumeAtDoseView = ViewLauncherService.GetEditMetricView_HI();
                EventAggregator.GetEvent<ShowHIMetricEvent>().Publish(scoreMetric);
                return volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.MaxDose || scoreMetric.MetricType == MetricTypeEnum.MinDose || scoreMetric.MetricType == MetricTypeEnum.MeanDose)
            {
                EditDoseValueView volumeAtDoseView = ViewLauncherService.GetEditMetricView_DoseValue();
                EventAggregator.GetEvent<ShowDoseValueMetricEvent>().Publish(scoreMetric);
                return volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.VolumeAtDose || scoreMetric.MetricType == MetricTypeEnum.VolumeOfRegret)
            {
                EditVolumeAtDoseView volumeAtDoseView = ViewLauncherService.GetEditMetricView_VolumeAtDose();
                EventAggregator.GetEvent<ShowVolumeAtDoseMetricEvent>().Publish(scoreMetric);
                return volumeAtDoseView;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

    }
}
