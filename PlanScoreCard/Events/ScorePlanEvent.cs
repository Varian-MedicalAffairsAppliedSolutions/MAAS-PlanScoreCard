﻿using PlanScoreCard.Models.Internals;
using Prism.Events;
using System.Collections.Generic;

namespace PlanScoreCard.Events
{
    public class ScorePlanEvent:PubSubEvent<List<ScoreTemplateModel>>
    {
    }
}
