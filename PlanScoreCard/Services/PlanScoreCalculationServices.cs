using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace PlanScoreCard.Services
{
    public static class PlanScoreCalculationServices
    {
        internal static double CalculatePKPosition(List<PlanScoreColorModel> Colors, bool increasing, double score)
        {
            if (increasing)
            {
                if (Colors.IndexOf(Colors.LastOrDefault(x => score >= x.ColorValue)) != -1)
                {
                    int index = Colors.IndexOf(Colors.LastOrDefault(x => score >= x.ColorValue));
                    if (index == Colors.Count() - 1)
                    {
                        return 60.0 * (double)Colors.Count();
                    }
                    else
                    {
                        double pkNextValue = Colors.ElementAt(index + 1).ColorValue;
                        double pkValue = Colors.ElementAt(index).ColorValue;
                        double position = (double)index * 60.0 + (score - pkValue) * (60 / (pkNextValue - pkValue));
                        //(60.0*(pkNextValue-spoint_value.Score)/(pkNextValue-PKColors.ElementAt(index).PKColorValue));
                        return position;
                    }
                }
                else { return 60.0 * Colors.Count(); }//the score must be higher than the template allows (maybe not possible). 
            }
            else
            {
                if (Colors.IndexOf(Colors.FirstOrDefault(x => x.ColorValue <= score)) != 0)
                {
                    int index = Colors.IndexOf(Colors.FirstOrDefault(x => x.ColorValue <= score));
                    if (index == 0)
                    {
                        return 0.0;
                    }
                    else
                    {
                        double pkPrevValue = Colors.ElementAt(index - 1).ColorValue;
                        double pkValue = Colors.ElementAt(index).ColorValue;
                        var position = (double)index * 60 + (pkPrevValue - score) * (60 / (pkPrevValue - pkValue));
                        return position;
                    }
                }
                else
                {
                    return 0.0;
                }
            }
        }
        /// <summary>
        /// Find the proper volume from the DVH
        /// </summary>
        /// <param name="template">Scoring Template</param>
        /// <param name="dvh_body">DVH of the body -- used in certain metrics</param>
        /// <param name="dvh">DVH of the structure</param>
        /// <param name="body_vol">The volume of the body DVH at the cutpoint.</param>
        /// <param name="target_vol">The volume of the structure DVH at the cutpoint</param>
        internal static void GetVolumesFromDVH(ScoreTemplateModel template, DVHData dvh_body, DVHData dvh, out double body_vol, out double target_vol)
        {
            if (template.InputUnit != dvh.MaxDose.UnitAsString)
            {
                if (template.InputUnit == "Gy")
                {
                    body_vol = dvh_body.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue * 100).Volume;
                    target_vol = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue * 100.0).Volume;
                }
                else
                {
                    body_vol = dvh_body.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue / 100.0).Volume;
                    target_vol = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue / 100.0).Volume;
                }

            }
            else
            {
                body_vol = dvh_body.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue).Volume;
                target_vol = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue).Volume;
            }
        }
        /// <summary>
        /// Determing metricText from template
        /// </summary>
        /// <param name="template">Score template</param>
        internal static string GetMetricTextFromTemplate(ScoreTemplateModel template)
        {
            if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.DoseAtVolume)
            {
                return $"Dose at {template.InputValue}{template.InputUnit} [{template.OutputUnit}]";
            }
            else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.VolumeAtDose)
            {
                return $"Volume at {template.InputValue}{template.InputUnit} [{template.OutputUnit}]";
            }
            else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.VolumeOfRegret)
            {
                return $"Volume of Regret [{template.InputValue}{template.InputUnit}] [{template.OutputUnit}]";
            }
            else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.ConformationNumber)
            {
                return $"Conformation Number at [{template.InputValue}{template.InputUnit}]";
            }
            else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.HomogeneityIndex)
            {
                return $"Homogeneity Index [{template.HI_HiValue} - {template.HI_LowValue}]/{template.HI_Target}]";
            }
            else
            {
                return $"{template.MetricType} [{template.OutputUnit}]";
            }
        }
        /// <summary>
        /// Determine Volume Type from Templaste
        /// </summary>
        /// <param name="plan">The plan to get the DVH</param>
        /// <param name="template">The template used for calculating DVH</param>
        /// <param name="structure">Structure for the DVH</param>
        /// <returns>The DVH of the structure</returns>
        internal static DVHData GetDVHForVolumeType(PlanningItem plan, ScoreTemplateModel template, Structure structure, double _dvhResolution)
        {
            return plan.GetDVHCumulativeData(structure,
                template.InputUnit.Contains("%") ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                template.OutputUnit.Contains("%") ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3,
               _dvhResolution);
        }
        /// <summary>
        /// Find score by interpolation
        /// </summary>
        /// <param name="scorePoints">Scoring function.</param>
        /// <param name="increasing">Scoring function increasing or decreasing.</param>
        /// <param name="value">Metric Value</param>
        /// <returns></returns>
        internal static double GetScore(List<ScorePointInternalModel> scorePoints, bool increasing, double value)
        {
            if (scorePoints.Count() == 0) { return 0; }
            else if (scorePoints.Count() == 1) { return scorePoints.First().Score; }
            if (!increasing)
            {
                if (value <= scorePoints.Min(x => x.PointX))
                {
                    return scorePoints.Max(x => x.Score);
                }
                else if (value >= scorePoints.Max(x => x.PointX))
                {
                    return scorePoints.Min(x => x.Score);
                }
                else
                {
                    //linearly interpolate. 
                    var pbefore = scorePoints.OrderBy(x => x.PointX).LastOrDefault(x => x.PointX <= value);
                    var pafter = scorePoints.OrderBy(x => x.PointX).First(x => x.PointX >= value);
                    return (double)pbefore.Score + (value - (double)pbefore.PointX) * (((double)pafter.Score - (double)pbefore.Score) / ((double)pafter.PointX - (double)pbefore.PointX));
                }
            }
            else
            {
                if (value >= scorePoints.Max(x => x.PointX))
                {
                    return scorePoints.Max(x => x.Score);
                }
                else if (value <= scorePoints.Min(x => x.PointX))
                {
                    return scorePoints.Min(x => x.Score);
                }
                else
                {
                    //linearly interpolate. 
                    var pbefore = scorePoints.OrderBy(x => x.PointX).LastOrDefault(x => x.PointX <= value);
                    var pafter = scorePoints.OrderBy(x => x.PointX).First(x => x.PointX >= value);
                    return pbefore.Score + (value - pbefore.PointX) * ((pafter.Score - pbefore.Score) / (pafter.PointX - pbefore.PointX));
                }
            }

        }
    }
}
