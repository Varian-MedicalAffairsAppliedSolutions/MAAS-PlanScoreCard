using PlanScoreCard.ViewModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlanScoreCard.Models.Internals
{
    public static class ScoreTemplateBuilder
    {
        private static List<StructureModel> _structures;

        public static List<ScoreTemplateModel> Build(List<ScoreMetricViewModel> scoreMetrics,
            List<StructureModel> structures)
        {
            List<ScoreTemplateModel> scoreTemplates = new List<ScoreTemplateModel>();
            _structures = structures;
            foreach (var scoreMetric in scoreMetrics)
            {
                if (scoreMetric.ScoreMetric != null)
                {
                    if (scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
                    {
                        scoreTemplates.Add(new ScoreTemplateModel(
                            scoreMetric.ScoreMetric.Structure,
                            scoreMetric.ScoreMetric.MetricType,
                            Convert.ToDouble(scoreMetric.ScoreMetric.HI_Hi),
                            Convert.ToDouble(scoreMetric.ScoreMetric.HI_Lo),
                            String.IsNullOrEmpty(scoreMetric.ScoreMetric.HI_Target) ? 0.0 : Convert.ToDouble(scoreMetric.ScoreMetric.HI_Target),
                            String.IsNullOrEmpty(scoreMetric.ScoreMetric.InputUnit) ? "cGy" : scoreMetric.ScoreMetric.InputUnit,
                            GetInternalScorePoint(scoreMetric.ScoreMetric.ScorePoints)));
                    }
                    else
                    {
                        scoreTemplates.Add(new ScoreTemplateModel(
                            scoreMetric.ScoreMetric.Structure,
                            scoreMetric.ScoreMetric.MetricType,
                            GetInputValue(scoreMetric),
                            GetInputUnit(scoreMetric),
                            GetOutputUnit(scoreMetric),
                            GetInternalScorePoint(scoreMetric.ScoreMetric.ScorePoints)));
                    }
                }
                //scoreMetric.ScoreMetric.ScorePoints.ToList()));
            }
            return scoreTemplates;
        }

        private static List<ScorePointInternalModel> GetInternalScorePoint(ObservableCollection<ScorePointModel> scorePoints)
        {
            List<ScorePointInternalModel> scores = new List<ScorePointInternalModel>();
            foreach (var score in scorePoints)
            {
                scores.Add(new ScorePointInternalModel((double)score.PointX, score.Score, score.bMetricChecked, score.Colors));
            }
            return scores;
        }

        private static string GetOutputUnit(ScoreMetricViewModel scoreMetric)
        {
            if (scoreMetric.ScoreMetric != null)
            {
                return scoreMetric.ScoreMetric.OutputUnit;
            }
            //if (scoreMetric._doseAtVolumeViewModel != null)
            //{
            //    return scoreMetric._doseAtVolumeViewModel.SelectedDoseUnit;
            //}
            //if (scoreMetric._volumeAtDoseViewModel != null)
            //{
            //    return scoreMetric._volumeAtDoseViewModel.SelectedVolumeUnit;
            //}
            //if (scoreMetric._doseValueViewModel != null)
            //{
            //    return scoreMetric._doseValueViewModel.SelectedDoseUnit;
            //}
            else { return String.Empty; }
        }

        private static string GetInputUnit(ScoreMetricViewModel scoreMetric)
        {
            if (scoreMetric.ScoreMetric != null)
            {
                return scoreMetric.ScoreMetric.InputUnit;
            }
            //if (scoreMetric._volumeAtDoseViewModel != null)
            //{
            //    return scoreMetric._volumeAtDoseViewModel.SelectedDoseUnit;
            //}
            else { return String.Empty; }
        }

        private static double GetInputValue(ScoreMetricViewModel scoreMetric)
        {
            //cannot use doseAtVolumeViewModel because it is the current setting in the navigation panel at the top
            //instead use scoreMetric._scoreMetric.Volume.
            if (scoreMetric.ScoreMetric != null)
            {
                return Convert.ToDouble(scoreMetric.ScoreMetric.InputValue);
            }
            else
            {
                return Double.NaN;
            }
        }
        public static List<ScoreMetricViewModel> GetScoreMetricsFromTemplate(List<ScoreTemplateModel> scoreTemplates,
            IEventAggregator _eventAggregator, int score_newId,
            List<StructureModel> structures)
        {
            _structures = structures;
            var ScoreMetrics = new List<ScoreMetricViewModel>();
            foreach (var template in scoreTemplates)
            {
                var scoreMetricVM = new ScoreMetricViewModel(
                    new DoseAtVolumeViewModel()
                    {
                        SelectedDoseUnit = template.OutputUnit,
                        SelectedVolumeUnit = template.InputUnit,
                        Volume = template.InputValue.ToString()
                    },
                    new VolumeAtDoseViewModel()
                    {
                        SelectedDoseUnit = template.InputUnit,
                        SelectedVolumeUnit = template.OutputUnit,
                        Dose = template.InputValue.ToString()
                    },
                    new DoseValueViewModel()
                    {
                        SelectedDoseUnit = template.OutputUnit
                    },
                    new HIViewModel
                    {
                        HI_HiValue = template.HI_HiValue,
                        HI_LowValue = template.HI_LowValue,
                        TargetValue = template.HI_Target,
                        SelectedDoseUnit = template.HI_TargetUnit
                    },
                    new CIViewModel()
                    {
                        SelectedDoseUnit = template.InputUnit,
                        Dose = template.InputValue.ToString()
                    },
                    (MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType),
                    score_newId,
                    template.Structure,
                    _structures,
                    _eventAggregator
                    );
                SetScorePoints(score_newId, template, scoreMetricVM, _eventAggregator);
                ScoreMetrics.Add(scoreMetricVM);
                score_newId++;
            }
            return ScoreMetrics;
        }

        private static void SetScorePoints(int score_newId, ScoreTemplateModel template, ScoreMetricViewModel scoreMetricVM, IEventAggregator eventAggregator)
        {
            int scorePointId = 0;
            foreach (var scorepoint in template.ScorePoints)
            {
                //var smetric = new ScorePointModel(score_newId, scorePointId, eventAggregator);
                scoreMetricVM.ScoreMetric.ScorePoints.Add(new ScorePointModel(score_newId, scorePointId, eventAggregator));
                scoreMetricVM.ScoreMetric.ScorePoints.Last().PointX = Convert.ToDecimal(scorepoint.PointX);
                scoreMetricVM.ScoreMetric.ScorePoints.Last().Score = scorepoint.Score;
                scoreMetricVM.ScoreMetric.ScorePoints.Last().bMetricChecked = scorepoint.Variation;
                if (scorepoint.Colors.Count() > 2)
                {
                    //PlanScoreColorModel scoreColor =
                    //scoreMetricVM.ScoreMetric.ScorePoints.Last().BackGroundBrush = scoreColor.PlanScoreBackgroundColor;
                    scoreMetricVM.ScoreMetric.ScorePoints.Last().Colors = new PlanScoreColorModel(new List<double>{
                     scorepoint.Colors.First(),
                     scorepoint.Colors.ElementAt(1),
                     scorepoint.Colors.ElementAt(2) }
                    , scorepoint.Label);
                }
                //scoreMetricVM.ScoreMetric.ScorePoints.Add(smetric);
                scorePointId++;
            }
        }

    }
}
