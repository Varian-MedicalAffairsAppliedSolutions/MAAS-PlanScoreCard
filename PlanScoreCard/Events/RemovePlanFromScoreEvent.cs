using PlanScoreCard.Models;
using Prism.Events;

namespace PlanScoreCard.Events
{
    internal class RemovePlanFromScoreEvent:PubSubEvent<PlanModel>
    {
    }
}
