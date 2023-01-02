using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class PlanModelOverride
    {
        public string PlanId { get; set; }
        public string CourseId { get; set; }
        public string PatientId { get; set; }
        public int TemplateMetricId { get; set; }
        public string TemplateStructureId { get; set; }
        public string StructureId { get; set; }
        public string MatchedStructureId { get; set; }
    }
}
