using PlanScoreCard.Models.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class ScoreCardModel
    {
        public string Name { get; set; }
        public string SiteGroup { get; set; }
        public List<ScoreTemplateModel> ScoreMetrics { get; set; }

        public ScoreCardModel(string name, string siteGroup, List<ScoreTemplateModel> scoreMetrics)
        {
            Name = name; 
            SiteGroup = siteGroup;
            ScoreMetrics = scoreMetrics;
        }

    }
}
