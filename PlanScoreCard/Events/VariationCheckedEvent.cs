using Prism.Events;
using System;

namespace PlanScoreCard.Events
{
    public class VariationCheckedEvent:PubSubEvent<Tuple<int,int>>
    {
    }
}
