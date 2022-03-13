using PlanScoreCard.Models;
using Prism.Events;
using System.Collections.Generic;

namespace PlanScoreCard.Events.Plugins
{
    public class PlanToPluginEvent:PubSubEvent<List<PlanModel>>
    {
    }
}
