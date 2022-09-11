using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    /// <summary>
    /// Class for importing patients.
    /// </summary>
    public class PatientPlanModel
    {
        public string PlanId { get; set; }
        public string PatientId { get; set; }
        public string CourseId { get; set; }
    }
}
