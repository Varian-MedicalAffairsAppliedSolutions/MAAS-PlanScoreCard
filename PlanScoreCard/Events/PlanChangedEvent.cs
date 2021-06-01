using PlanScoreCard.Models;
using Prism.Events;
using System.Collections.Generic;

namespace PlanScoreCard.Events
{
    public class PlanChangedEvent:PubSubEvent<List<PlanModel>>
    {
    }
}
