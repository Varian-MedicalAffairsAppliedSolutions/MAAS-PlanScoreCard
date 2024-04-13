using Prism.Events;
using System;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Events
{
    public class StructureGeneratedOnScoreEvent:PubSubEvent<Tuple<StructureSet,Structure>>
    {
    }
}
