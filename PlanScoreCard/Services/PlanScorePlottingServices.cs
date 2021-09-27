using OxyPlot;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.ViewModels;
using System;
using System.Linq;

namespace PlanScoreCard.Services
{
    public static class PlanScorePlottingServices
    {
        /// <summary>
        /// Return Color based on calculated score
        /// </summary>
        /// <param name="score">Current Metric SCore</param>
        /// <param name="template">Template with variation values</param>
        /// <returns></returns>
        internal static OxyColor GetColorFromMetric(double score, ScoreTemplateModel template)
        {
            if (score == template.ScorePoints.Max(x => x.Score))
            {
                return OxyColors.Blue;
            }
            else if (score == template.ScorePoints.Min(x => x.Score))
            {
                return OxyColors.Red;
            }
            if (template.ScorePoints.Any(x => x.Variation))
            {
                if (score > template.ScorePoints.SingleOrDefault(x => x.Variation).Score)
                {
                    return OxyColors.Green;
                }
                else
                {
                    return OxyColors.Yellow;
                }
            }
            return OxyColors.Green;
        }
        /// <summary>
        /// Determine axis title from metric type
        /// </summary>
        /// <param name="metricType">Metric Type Enum</param>
        /// <returns></returns>
        internal static string GetPlotXAxisTitle(MetricTypeEnum metricType,
            DoseAtVolumeViewModel _doseAtVolumeViewModel,
            VolumeAtDoseViewModel _volumeAtDoseViewModel,
            DoseValueViewModel _doseValueViewModel)
        {
            if (metricType == MetricTypeEnum.DoseAtVolume)

            {
                return $"Dose [{_doseAtVolumeViewModel.SelectedDoseUnit}]";
            }
            else if (metricType == MetricTypeEnum.MaxDose || metricType == MetricTypeEnum.MinDose || metricType == MetricTypeEnum.MeanDose)
            {
                return $"Dose [{_doseValueViewModel.SelectedDoseUnit}]";
            }
            else if (metricType == MetricTypeEnum.VolumeAtDose || metricType == MetricTypeEnum.VolumeOfRegret)
            {
                return $"Volume [{_volumeAtDoseViewModel.SelectedVolumeUnit}]";
            }
            else if (metricType == MetricTypeEnum.ConformationNumber)
            {
                return $"Conformation Number";
            }
            else if (metricType == MetricTypeEnum.HomogeneityIndex)
            {
                return $"Homogeneity Index";
            }
            else
            {
                return "Undefined";
            }
        }
        /// <summary>
        /// Determine Plot title from metric
        /// </summary>
        /// <param name="metricType">Metric Type enum.</param>
        /// <returns></returns>
        internal static string GetPlotTitle(MetricTypeEnum metricType,
            DoseAtVolumeViewModel _doseAtVolumeViewModel,
            VolumeAtDoseViewModel _volumeAtDoseViewModel)
        {
            switch (metricType)
            {
                case MetricTypeEnum.DoseAtVolume:
                    return $"Score for Dose at {_doseAtVolumeViewModel.Volume}{_doseAtVolumeViewModel.SelectedVolumeUnit}";
                case MetricTypeEnum.VolumeAtDose:
                    return $"Score for Volume at {_volumeAtDoseViewModel.Dose}{_volumeAtDoseViewModel.SelectedDoseUnit}";
                case MetricTypeEnum.VolumeOfRegret:
                    return $"Volume of Regret at {_volumeAtDoseViewModel.Dose}{_volumeAtDoseViewModel.SelectedDoseUnit}";
                case MetricTypeEnum.ConformationNumber:
                    return $"Conformation Number at {_volumeAtDoseViewModel.Dose}{_volumeAtDoseViewModel.SelectedDoseUnit}";
                case MetricTypeEnum.HomogeneityIndex:
                    return $"Homogeneity Index Score";
                case MetricTypeEnum.MinDose:
                    return $"Min Dose Score";
                case MetricTypeEnum.MeanDose:
                    return $"Mean Dose Score";
                case MetricTypeEnum.MaxDose:
                    return $"Max Dose Score";
                default:
                    return $"Undefined Metric";
            }
        }

        internal static string GetPlotXAxisTitle(MetricTypeEnum metricType, ScoreMetricModel scoreMetricModel)
        {
            if (metricType == MetricTypeEnum.DoseAtVolume)

            {
                return $"Dose [{scoreMetricModel.OutputUnit}]";
            }
            else if (metricType == MetricTypeEnum.MaxDose || metricType == MetricTypeEnum.MinDose || metricType == MetricTypeEnum.MeanDose)
            {
                return $"Dose [{scoreMetricModel.OutputUnit}]";
            }
            else if (metricType == MetricTypeEnum.VolumeAtDose || metricType == MetricTypeEnum.VolumeOfRegret)
            {
                return $"Volume [{scoreMetricModel.OutputUnit}]";
            }
            else if (metricType == MetricTypeEnum.ConformationNumber)
            {
                return $"Conformation Number";
            }
            else if (metricType == MetricTypeEnum.HomogeneityIndex)
            {
                return $"Homogeneity Index";
            }
            else
            {
                return "Undefined";
            }
        }

        internal static string GetPlotTitle(MetricTypeEnum metricType, ScoreMetricModel scoreMetricModel)
        {
            switch (metricType)
            {
                case MetricTypeEnum.DoseAtVolume:
                    return $"Score for Dose at {scoreMetricModel.InputValue}{scoreMetricModel.InputUnit}";
                case MetricTypeEnum.VolumeAtDose:
                    return $"Score for Volume at {scoreMetricModel.InputValue}{scoreMetricModel.InputUnit}";
                case MetricTypeEnum.VolumeOfRegret:
                    return $"Volume of Regret at {scoreMetricModel.InputValue}{scoreMetricModel.InputUnit}";
                case MetricTypeEnum.ConformationNumber:
                    return $"Conformation Number at {scoreMetricModel.InputValue}{scoreMetricModel.InputUnit}";
                case MetricTypeEnum.HomogeneityIndex:
                    return $"Homogeneity Index Score";
                case MetricTypeEnum.MinDose:
                    return $"Min Dose Score";
                case MetricTypeEnum.MeanDose:
                    return $"Mean Dose Score";
                case MetricTypeEnum.MaxDose:
                    return $"Max Dose Score";
                default:
                    return $"Undefined Metric";
            }
        }
    }
}
