using System.Collections.Generic;

namespace PlanScoreCard.Models.Proknow
{
    //proknow score template model
    public class pk_computedmodel
    {
        public pk_typeEnum type { get; set; }
        public string roi_name { get; set; }
        public double? arg_1 { get; set; }
        public double? arg_2 { get; set; }
        public List<pk_objectiveModel> objectives { get; set; }
    }
}
