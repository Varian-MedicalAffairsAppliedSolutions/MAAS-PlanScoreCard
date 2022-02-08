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
                            scoreMetric.ScoreMetric.MetricComment,
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
                            scoreMetric.ScoreMetric.MetricComment,
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

        public static List<ScoreTemplateModel> Build(List<ScoreMetricModel> scoreMetrics,
            List<StructureModel> structures)
        {
            List<ScoreTemplateModel> scoreTemplates = new List<ScoreTemplateModel>();
            _structures = structures;
            foreach (var scoreMetric in scoreMetrics)
            {
                if (scoreMetric != null)
                {
                    if (scoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
                    {
                        scoreTemplates.Add(new ScoreTemplateModel(
                            scoreMetric.Structure,
                            scoreMetric.MetricType,
                            scoreMetric.MetricComment,
                            Convert.ToDouble(scoreMetric.HI_Hi),
                            Convert.ToDouble(scoreMetric.HI_Lo),
                            String.IsNullOrEmpty(scoreMetric.HI_Target) ? 0.0 : Convert.ToDouble(scoreMetric.HI_Target),
                            String.IsNullOrEmpty(scoreMetric.InputUnit) ? "cGy" : scoreMetric.InputUnit,
                            GetInternalScorePoint(scoreMetric.ScorePoints)));
                    }
                    else
                    {
                        scoreTemplates.Add(new ScoreTemplateModel(
                            scoreMetric.Structure,
                            scoreMetric.MetricType,
                            scoreMetric.MetricComment,
                            GetInputValue(scoreMetric),
                            GetInputUnit(scoreMetric),
                            GetOutputUnit(scoreMetric),
                            GetInternalScorePoint(scoreMetric.ScorePoints)));
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

        private static string GetOutputUnit(ScoreMetricModel scoreMetric)
        {
            if (scoreMetric != null)
            {
                return scoreMetric.OutputUnit;
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

        private static string GetInputUnit(ScoreMetricModel scoreMetric)
        {
            if (scoreMetric != null)
            {
                return scoreMetric.InputUnit;
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

        private static double GetInputValue(ScoreMetricModel scoreMetric)
        {
            //cannot use doseAtVolumeViewModel because it is the current setting in the navigation panel at the top
            //instead use scoreMetric._scoreMetric.Volume.
            if (scoreMetric != null)
            {
                return Convert.ToDouble(scoreMetric.InputValue);
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


        // Called from the ScoreCard Editor 
        public static List<ScoreMetricModel> GetScoreCardMetricsFromTemplate(List<ScoreTemplateModel> scoreTemplates, IEventAggregator _eventAggregator, int score_newId, List<StructureModel> structures)
        {
            _structures = structures;
            var ScoreMetrics = new List<ScoreMetricModel>();
            int rankCounter = 1;
            foreach (var template in scoreTemplates)
            {
                ScoreMetricModel scoreMetric = new ScoreMetricModel(_eventAggregator);
                scoreMetric.EventAggregator = _eventAggregator;
                scoreMetric.CanReorder = false;

                // Structures
                foreach (StructureModel structure in structures)
                    scoreMetric.Structures.Add(structure);

                // Rank
                scoreMetric.Id = rankCounter;
                rankCounter++;

                // Metric Type
                scoreMetric.MetricType = (MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType);

                // Structure
                scoreMetric.Structure = template.Structure;
                scoreMetric.Structure.TemplateStructureId = template.Structure.TemplateStructureId;
                scoreMetric.MetricComment = template.MetricComment;

                // Metric Type - Dependant Variables
                if (scoreMetric.MetricType == MetricTypeEnum.DoseAtVolume || scoreMetric.MetricType == MetricTypeEnum.VolumeAtDose|| scoreMetric.MetricType == MetricTypeEnum.ConformationNumber || scoreMetric.MetricType == MetricTypeEnum.VolumeOfRegret)
                {
                    scoreMetric.OutputUnit = template.OutputUnit;
                    scoreMetric.InputUnit = template.InputUnit;
                    scoreMetric.InputValue = template.InputValue.ToString();
                }
                else if (scoreMetric.MetricType == MetricTypeEnum.MaxDose || scoreMetric.MetricType == MetricTypeEnum.MeanDose || scoreMetric.MetricType == MetricTypeEnum.MinDose)
                {
                    scoreMetric.OutputUnit = template.OutputUnit;
                }
                else if (scoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
                {
                    scoreMetric.HI_Hi = template.HI_HiValue.ToString();
                    scoreMetric.HI_Lo = template.HI_LowValue.ToString();
                    scoreMetric.HI_Target = template.HI_Target.ToString();
                    scoreMetric.OutputUnit = template.HI_TargetUnit;
                }
                else if (scoreMetric.MetricType == MetricTypeEnum.ConformityIndex)
                {
                    scoreMetric.OutputUnit = template.InputUnit;
                    scoreMetric.InputValue = template.InputValue.ToString();
                }

                // Metric Text
                scoreMetric.MetricText = GetScoreMetricText(scoreMetric);

                // Set the ScorePoints 
                int scorePointId = 0;
                foreach (ScorePointInternalModel scorepoint in template.ScorePoints)
                {
                    ScorePointModel pointModel = new ScorePointModel(score_newId, scorePointId, _eventAggregator);
                    pointModel.PointX = Convert.ToDecimal(scorepoint.PointX);
                    pointModel.Score = scorepoint.Score;
                    pointModel.bMetricChecked = scorepoint.Variation;

                    if (scorepoint.Colors.Count() > 0)
                    {
                        pointModel.PlanScoreBackgroundColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(Convert.ToByte(scorepoint.Colors.ElementAt(0)),
                            Convert.ToByte(scorepoint.Colors.ElementAt(1)),
                            Convert.ToByte(scorepoint.Colors.ElementAt(2))));

                        if (scorepoint.Colors.Count() > 2)
                            pointModel.Colors = new PlanScoreColorModel(new List<double> { scorepoint.Colors.First(), scorepoint.Colors.ElementAt(1), scorepoint.Colors.ElementAt(2) }, scorepoint.Label);
                    }
                    
                    scoreMetric.ScorePoints.Add(pointModel);
                    scorePointId++;
                }


                scoreMetric.CanReorder = true;
                ScoreMetrics.Add(scoreMetric);
                score_newId++;
            }
            return ScoreMetrics;
        }

        private static string GetScoreMetricText(ScoreMetricModel scoreMetric)
        {
            switch (scoreMetric.MetricType)
            {
                case MetricTypeEnum.DoseAtVolume:
                    return $"Dose at {scoreMetric.InputValue}{scoreMetric.InputUnit}";
                case MetricTypeEnum.VolumeAtDose:
                    return $"Volume at {scoreMetric.InputValue}{scoreMetric.InputUnit}";
                case MetricTypeEnum.MinDose:
                    return $"Min Dose [{scoreMetric.OutputUnit}]";
                case MetricTypeEnum.MeanDose:
                    return $"Mean Dose [{scoreMetric.OutputUnit}]";
                case MetricTypeEnum.MaxDose:
                    return $"Max Dose [{scoreMetric.OutputUnit}]";
                case MetricTypeEnum.VolumeOfRegret:
                    return $"Vol of regret at {scoreMetric.InputValue}{scoreMetric.InputUnit}";
                case MetricTypeEnum.ConformationNumber:
                    return $"Conf No. at {scoreMetric.InputValue}{scoreMetric.InputUnit}";
                case MetricTypeEnum.HomogeneityIndex:
                    return $"HI [D{scoreMetric.HI_Hi}-D{scoreMetric.HI_Lo}]/{scoreMetric.HI_Target}";
                case MetricTypeEnum.ConformityIndex:
                    return $"CI [{scoreMetric.InputValue} [{scoreMetric.InputUnit}]]";
                default:
                    return "Undefined Metric";
            }
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
