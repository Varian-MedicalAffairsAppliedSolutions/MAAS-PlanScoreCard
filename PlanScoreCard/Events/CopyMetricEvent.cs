using PlanScoreCard.ViewModels;
using Prism.Events;

namespace PlanScoreCard.Events
{
    public class CopyMetricEvent:PubSubEvent<ScoreMetricViewModel>
    {
    }
}
