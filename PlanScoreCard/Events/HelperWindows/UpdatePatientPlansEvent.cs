using PlanScoreCard.Models;
using Prism.Events;
using System.Collections.Generic;

namespace PlanScoreCard.Events.HelperWindows
{
    internal class UpdatePatientPlansEvent:PubSubEvent<List<PatientPlanSearchModel>>
    {
    }
}
