using System.Collections.Generic;

namespace PlanScoreCard.Models.Proknow
{
    public class pk_objectiveModel
    {
        public double? max { get; set; }
        public double? min { get; set; }
        public List<double> color { get; set; }
        public string label { get; set; }

    }
}
