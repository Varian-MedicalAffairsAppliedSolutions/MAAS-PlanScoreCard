using Prism.Events;
using System;

namespace PlanScoreCard.Events
{
    public class PointUpEvent:PubSubEvent<Tuple<int,int>>
    {
    }
}
