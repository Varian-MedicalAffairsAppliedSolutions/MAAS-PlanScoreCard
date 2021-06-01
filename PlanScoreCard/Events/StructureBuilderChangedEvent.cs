using PlanScoreCard.Models;
using Prism.Events;

namespace PlanScoreCard.Events
{
    public class StructureBuilderChangedEvent:PubSubEvent<StructureOperationModel>
    {
    }
}
