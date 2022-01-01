using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlanScoreCard.Models.Internals
{
    public static class EPeerReviewScoreModel
    {
        /// <summary>
        /// read file and parse string
        /// </summary>
        /// <param name="filename">filename.</param>
        /// <returns></returns>
        public static List<ScoreTemplateModel> GetScoreTemplateFromCSV(string filename)
        {
            List<ScoreTemplateModel> scoreTemplates = new List<ScoreTemplateModel>();
            var line_count = 0;
            foreach (var line in File.ReadAllLines(filename))
            {
                if (line_count != 0)
                {
                    var structureId = line.Split(',').First();
                    var structureCode = line.Split(',').ElementAt(1);
                    //alias is not being used in this code... possible for implementation.
                    string inputUnit = String.Empty;
                    string outputUnit = String.Empty;
                    double inputValue = 0.0;
                    var metricType = MetricTypeEnum.Undefined;
                    ParseDVHObjective(line.Split(',').ElementAt(3), out inputUnit, out outputUnit, out inputValue, out metricType);
                    List<ScorePointInternalModel> scores = ParseScores(line.Split(',').Last());
                    scoreTemplates.Add(new ScoreTemplateModel(
                        new StructureModel
                        {
                            StructureId = structureId,
                            StructureCode = structureCode
                        },
                        metricType,
                        String.Empty,
                        inputValue,
                        inputUnit,
                        outputUnit,
                        scores.OrderBy(x => x.PointX).ToList()));
                }
                line_count++;
            }
            return scoreTemplates;
        }
        /// <summary>
        /// Get scores from the scoring function string
        /// </summary>
        /// <param name="v">Score function string for EPR.</param>
        /// <returns></returns>
        internal static List<ScorePointInternalModel> ParseScores(string v)
        {
            List<ScorePointInternalModel> scorePoints = new List<ScorePointInternalModel>();
            int score_count = 0;
            foreach (var score in v.Split(')'))
            {
                if (!String.IsNullOrEmpty(score))
                {
                    double point = 0.0;
                    double.TryParse(score.Split(';').First().TrimStart('('), out point);
                    double val = 0.0;
                    double.TryParse(score.Split(';').Last(), out val);
                    bool variation = false;
                    if (score_count == 1) { variation = true; }
                    scorePoints.Add(new ScorePointInternalModel(point, val, variation, null));
                    score_count++;
                }
            }
            return scorePoints;
        }
        /// <summary>
        /// Break the DVH string into constiuient components
        /// </summary>
        /// <param name="dvhObj">DVH Metric String</param>
        /// <param name="inputUnit">Returned Input Unit</param>
        /// <param name="outputUnit">Returned output unit</param>
        /// <param name="inputValue">returned input value</param>
        /// <param name="metricType">returned metric type</param>
        internal static void ParseDVHObjective(string dvhObj, out string inputUnit, out string outputUnit, out double inputValue, out MetricTypeEnum metricType)
        {
            //throw new NotImplementedException();
            outputUnit = String.Empty;
            inputUnit = String.Empty;
            inputValue = 0.0;

            if (dvhObj.StartsWith("Mean"))
            {
                metricType = MetricTypeEnum.MeanDose;
                outputUnit = ParseOutputUnit(dvhObj);
                return;
            }
            else if (dvhObj.StartsWith("Max"))
            {
                metricType = MetricTypeEnum.MaxDose;
                outputUnit = ParseOutputUnit(dvhObj);
                return;
            }
            else if (dvhObj.StartsWith("Min"))
            {
                metricType = MetricTypeEnum.MinDose;
                outputUnit = ParseOutputUnit(dvhObj);
                return;
            }
            else if (dvhObj.StartsWith("D"))
            {
                int parse_index = dvhObj.Contains("cc") ?
                    dvhObj.IndexOf("cc") :
                    dvhObj.IndexOf("%");
                Double.TryParse(dvhObj.Substring(1, parse_index), out inputValue);
                metricType = MetricTypeEnum.DoseAtVolume;
                outputUnit = ParseOutputUnit(dvhObj);
                inputUnit = dvhObj.Contains("cc") ?
                    "cc" : "%";
                return;
            }
            else if (dvhObj.StartsWith("V"))
            {
                int parse_index = dvhObj.Contains("Gy") ?
                    dvhObj.Contains("cGy") ? dvhObj.IndexOf("cGy") : dvhObj.IndexOf("Gy") :
                    dvhObj.IndexOf("%");
                Double.TryParse(dvhObj.Substring(1, parse_index), out inputValue);
                metricType = MetricTypeEnum.VolumeAtDose;
                outputUnit = ParseOutputUnit(dvhObj);
                inputUnit = dvhObj.Contains("Gy") ? dvhObj.Contains("cGy") ? "cGy" : "Gy" : "%";
                return;
            }
            metricType = MetricTypeEnum.Undefined;
            return;
        }
        /// <summary>
        /// determines the dvh output unit from the string
        /// </summary>
        /// <param name="dvhObj">the metric string</param>
        /// <returns></returns>
        internal static string ParseOutputUnit(string dvhObj)
        {
            return dvhObj.Split('[').Last().Split(']').First();
        }
    }
}
