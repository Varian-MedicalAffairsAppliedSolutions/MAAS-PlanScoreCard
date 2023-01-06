using PlanScoreCard.Models.Internals;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanScoreCard.Models.Proknow
{
    public class PKModel
    {
        private IEventAggregator _eventAggregator;

        public List<pk_computedmodel> computed { get; set; }
        public List<pk_custommodel> custom { get; set; }
        public PKModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            computed = new List<pk_computedmodel>();
            custom = new List<pk_custommodel>();

        }
        public List<ScoreTemplateModel> ConvertToTemplate()
        {
            List<ScoreTemplateModel> scoreTemplates = new List<ScoreTemplateModel>();
            int metricNum = 0;
            foreach (var metric in computed)
            {

                string inputUnit = null;
                string outputUnit = null;
                MetricTypeEnum metricType = MetricTypeEnum.Undefined;
                GetMetricType(metric.type, out inputUnit, out outputUnit, out metricType);
                double inputValue = metric.arg_1 == null ? -1 : (double)metric.arg_1;
                List<ScorePointInternalModel> scores = GetScoresFromLabel(metric);
                ScoreTemplateModel stm = null;
                if (metricType == MetricTypeEnum.HomogeneityIndex)
                {
                    stm = new ScoreTemplateModel(metricNum,new StructureModel(_eventAggregator) { StructureId = metric.roi_name },
                        metricType,
                        String.Empty,
                        1.0,//PK always uses D1% as high dose value.
                        99.0,//PK always uses D99% as low dose value.
                        String.Empty,
                        inputValue,//PK puts expected target dose in arg1.
                        "Gy",//Dose unit seems to be in Gy.
                        scores);
                }
                else
                {
                    stm = new ScoreTemplateModel(metricNum,new StructureModel(_eventAggregator) { StructureId = metric.roi_name },
                    metricType,
                    String.Empty,
                    inputValue,
                    inputUnit,
                    outputUnit,
                    scores);
                }
                metricNum++;
                //stm.InputUnit = GetMetricUnit(metric.type);
                scoreTemplates.Add(stm);

            }
            return scoreTemplates;
        }

        private List<ScorePointInternalModel> GetScoresFromLabel(pk_computedmodel metric)
        {
            List<ScorePointInternalModel> scores = new List<ScorePointInternalModel>();
            if (metric.objectives != null)
            {
                foreach (var pk_objective in metric.objectives)
                {
                    var objective = pk_objective.max == null ? pk_objective.min : pk_objective.max;
                    var val = pk_objective.label.Contains('[')
                        ? Convert.ToDouble(pk_objective.label.Split('[').Last().Split(']').First())
                        : -1.0;
                    bool variation = pk_objective.label.StartsWith("ACCEPTABLE");
                    if (objective != null && val > -1.0)
                    {
                        var score = new ScorePointInternalModel((double)objective, val, variation, new PlanScoreColorModel(pk_objective.color, pk_objective.label));
                        //foreach(var color in pk_objective.color)
                        //{
                        //    score.Colors.Add(color);
                        //}
                        scores.Add(score);
                    }
                }
            }
            return scores;
        }

        private void GetMetricType(pk_typeEnum type, out string inputUnit, out string outputUnit, out MetricTypeEnum metricType)
        {
            switch (type)
            {
                case pk_typeEnum.DOSE_VOLUME_PERCENT_ROI:
                    inputUnit = "%";
                    outputUnit = "Gy";
                    metricType = MetricTypeEnum.DoseAtVolume;
                    //return "Dose at Volume";
                    break;
                case pk_typeEnum.DOSE_VOLUME_CC_ROI:
                    inputUnit = "CC";
                    outputUnit = "Gy";
                    metricType = MetricTypeEnum.DoseAtVolume;
                    break;
                case pk_typeEnum.VOLUME_PERCENT_DOSE_ROI:
                    inputUnit = "Gy";
                    outputUnit = "%";
                    metricType = MetricTypeEnum.VolumeAtDose;
                    break;
                case pk_typeEnum.VOLUME_CC_DOSE_ROI:
                    inputUnit = "Gy";
                    outputUnit = "CC";
                    metricType = MetricTypeEnum.VolumeAtDose;
                    break;
                case pk_typeEnum.MEAN_DOSE_ROI:
                    inputUnit = String.Empty;
                    outputUnit = "Gy";
                    metricType = MetricTypeEnum.MeanDose;
                    break;
                case pk_typeEnum.MAX_DOSE_ROI:
                    inputUnit = String.Empty;
                    outputUnit = "Gy";
                    metricType = MetricTypeEnum.MaxDose;
                    break;
                case pk_typeEnum.MIN_DOSE_ROI:
                    inputUnit = String.Empty;
                    outputUnit = "Gy";
                    metricType = MetricTypeEnum.MinDose;
                    break;
                case pk_typeEnum.VOLUME_OF_REGRET:
                    inputUnit = "Gy";
                    outputUnit = "CC";
                    metricType = MetricTypeEnum.VolumeOfRegret;
                    break;
                case pk_typeEnum.CONFORMATION_NUMBER:
                    inputUnit = "Gy";
                    outputUnit = "CC";
                    metricType = MetricTypeEnum.ConformationNumber;
                    break;
                case pk_typeEnum.HOMOGENEITY_INDEX:
                    inputUnit = "NA";
                    outputUnit = "NA";
                    metricType = MetricTypeEnum.HomogeneityIndex;
                    break;
                case pk_typeEnum.CONFORMALITY_INDEX:
                    inputUnit = "Gy";
                    outputUnit = "CC";
                    metricType = MetricTypeEnum.ConformityIndex;
                    break;
                default:
                    inputUnit = "NA";
                    outputUnit = "NA";
                    metricType = MetricTypeEnum.Undefined;
                    break;
            }
        }
    }
}
