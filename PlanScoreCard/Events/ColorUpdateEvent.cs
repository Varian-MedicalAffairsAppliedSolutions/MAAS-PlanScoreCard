using Prism.Events;
using System;

namespace PlanScoreCard.Events
{
    public class ColorUpdateEvent : PubSubEvent<Tuple<int, int, string, string>>
    {
    }
}
