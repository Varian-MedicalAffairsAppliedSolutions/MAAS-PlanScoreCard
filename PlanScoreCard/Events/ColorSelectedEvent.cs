using Prism.Events;
using System;

namespace PlanScoreCard.Events
{
    public class ColorSelectedEvent:PubSubEvent<Tuple<int,int,string,string>>
    {
    }
}
