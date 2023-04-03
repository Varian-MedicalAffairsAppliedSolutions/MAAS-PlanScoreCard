using PlanScoreCard.Models;
using Prism.Events;
using System.Collections.Generic;

namespace PlanScoreCard.Events.Plugins
{
    public class PlotUpdateEvent:PubSubEvent<List<PlanScoreModel>>
    {
    }
}
