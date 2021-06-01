using Prism.Events;
using System;

namespace PlanScoreCard.Events
{
    public class PointDownEvent:PubSubEvent<Tuple<int,int>>
    {
    }
}
