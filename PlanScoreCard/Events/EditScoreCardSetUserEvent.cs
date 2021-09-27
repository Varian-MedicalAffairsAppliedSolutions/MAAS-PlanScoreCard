using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Events
{
    public class EditScoreCardSetUserEvent : PubSubEvent<User>
    {
    }
}
