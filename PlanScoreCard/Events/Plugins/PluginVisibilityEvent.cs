using Prism.Events;
using System;

namespace PlanScoreCard.Events.Plugin
{
    public class PluginVisibilityEvent : PubSubEvent<bool>
    {
        internal void SubScribe(object onPluginVisible)
        {
            throw new NotImplementedException();
        }
    }
}
