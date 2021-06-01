using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PlanScoreCard.Models
{
    public class ScoreMetricModel
    {
        public MetricTypeEnum MetricType { get; set; }
        public StructureModel Structure { get; set; }

        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public ObservableCollection<ScorePointModel> ScorePoints { get; set; }
        public string InputValue { get; internal set; }
        public string InputUnit { get; internal set; }
        public string OutputUnit { get; internal set; }
        public string HI_Hi { get; internal set; }
        public string HI_Lo { get; internal set; }
        public string HI_Target { get; internal set; }
        public Dictionary<string, double> ScoreMetricLevelSettings { get; set; }

        public ScoreMetricModel()
        {
            ScorePoints = new ObservableCollection<ScorePointModel>();
            ScoreMetricLevelSettings = new Dictionary<string, double>();

        }
    }
}
