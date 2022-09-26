using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Models
{
    public class PatientPlanSearchModel
    {
        public string PatientId { get; set; }
        public string PatientName { get; set; }

        private IEventAggregator _eventAggregator;

        public List<PlanModel> Plans { get; set; }
        public PatientPlanSearchModel(Patient patient, IEventAggregator eventAggregator)
        {
            Plans = new List<PlanModel>();
            PatientId = patient.Id;
            PatientName = $"{patient.LastName}, {patient.FirstName}";
            _eventAggregator = eventAggregator;
            GetPlans(patient);
        }

        private void GetPlans(Patient patient)
        {
            foreach(var course in patient.Courses)
            {
                foreach(var plan in course.PlanSetups)
                {
                    var localPlan = new PlanModel(plan, _eventAggregator);
                    Plans.Add(localPlan);
                }
            }
        }
    }
}
