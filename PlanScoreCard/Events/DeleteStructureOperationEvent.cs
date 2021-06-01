using PlanScoreCard.Models;
using Prism.Events;

namespace PlanScoreCard.Events
{
    public class DeleteStructureOperationEvent:PubSubEvent<StructureOperationModel>
    {
    }
}
