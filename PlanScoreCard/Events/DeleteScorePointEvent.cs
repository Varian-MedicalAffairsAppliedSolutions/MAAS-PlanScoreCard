using Prism.Events;
using System;

namespace PlanScoreCard.Events
{
    public class DeleteScorePointEvent:PubSubEvent<Tuple<int,int>>
    {
    }
}
