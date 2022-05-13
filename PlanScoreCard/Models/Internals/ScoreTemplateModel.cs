using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models.Internals
{
    public class ScoreTemplateModel
    {
        public StructureModel Structure { get; set; }
        public int Rank { get; set; }
        public string MetricType { get; set; }
        public double InputValue { get; set; }
        public string InputUnit { get; set; }
        public string OutputUnit { get; set; }
        public double? AcceptableLevel { get; set; }
        public double? VariationLevel { get; set; }
        public double? FailLevel { get; set; }
        public double HI_HiValue { get; set; }
        public double HI_LowValue { get; set; }
        public double HI_Target { get; set; }
        public string HI_TargetUnit { get; set; }
        public string MetricComment { get; set; }
        public List<ScorePointInternalModel> ScorePoints { get; private set; }


        /// <summary>
        /// Constructor for all dose metrics other than HI. 
        /// </summary>
        /// <param name="structure">Structure Model includes Id and Code</param>
        /// <param name="metricType">Metric Type</param>
        /// <param name="inputValue">Input Numerical Value</param>
        /// <param name="inputUnit">Unit of Input VAlue</param>
        /// <param name="outputUnit">Unit of Output Value</param>
        /// <param name="scorePoints">Scoring curve</param>
        public ScoreTemplateModel(StructureModel structure, MetricTypeEnum metricType,
            string metricComment,
            double inputValue,
            string inputUnit,
            string outputUnit,
            List<ScorePointInternalModel> scorePoints)
        {
            Structure = structure;
            MetricType = metricType.ToString();
            InputValue = inputValue;
            InputUnit = inputUnit;
            OutputUnit = outputUnit;
            MetricComment = metricComment;
            ScorePoints = new List<ScorePointInternalModel>();
            foreach (var point in scorePoints)
            {
                ScorePoints.Add(point);
            }
        }
        /// <summary>
        /// Constructor for Homogeneity Index
        /// </summary>
        /// <param name="structure">Structure including Id and structure code</param>
        /// <param name="metricType">Metric Type</param>
        /// <param name="hi_HiValue">The high dose value of HI</param>
        /// <param name="hi_LowValue">The low dose value of HI</param>
        /// <param name="hi_Target">The denominator in HI calculation</param>
        /// <param name="hi_TargetUnit">Unit of target value</param>
        /// <param name="scorePoints">Scoring Function for HI</param>
        public ScoreTemplateModel(StructureModel structure, MetricTypeEnum metricType, string metricComment,
            double hi_HiValue,
            double hi_LowValue,
            string inputUnit,
            double hi_Target,
            string hi_TargetUnit,
            List<ScorePointInternalModel> scorePoints)
        {
            Structure = structure;
            MetricType = metricType.ToString();
            InputUnit = inputUnit;
            HI_HiValue = hi_HiValue;
            HI_LowValue = hi_LowValue;
            HI_Target = hi_Target;
            HI_TargetUnit = hi_TargetUnit;
            ScorePoints = new List<ScorePointInternalModel>();
            MetricComment = metricComment;
            foreach (var point in scorePoints)
            {
                ScorePoints.Add(point);
            }
        }

        public ScoreTemplateModel()
        {
            ScorePoints = new List<ScorePointInternalModel>();
        }

    }
}
