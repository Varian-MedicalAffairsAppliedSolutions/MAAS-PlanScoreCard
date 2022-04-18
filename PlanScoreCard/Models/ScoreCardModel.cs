using PlanScoreCard.Models.Internals;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class ScoreCardModel : BindableBase
    {
        public string Name { get; set; }
        public string SiteGroup { get; set; }
        public double DosePerFraction { get; set; }
        public int NumberOfFractions { get; set; }

        private List<ScoreTemplateModel> scoreMetrics;

        public List<ScoreTemplateModel> ScoreMetrics
        {
            get { return scoreMetrics; }
            set { SetProperty(ref scoreMetrics , value); }
        }

        public ScoreCardModel(string name, string siteGroup, double dosePerFraction, int numberOfFractions,List<ScoreTemplateModel> scoreMetrics)
        {
            Name = name; 
            SiteGroup = siteGroup;
            ScoreMetrics = scoreMetrics;
            DosePerFraction = dosePerFraction;
            NumberOfFractions = numberOfFractions;

            if (ScoreMetrics == null)
            {
                ScoreMetrics = new List<ScoreTemplateModel>();
            }
        }

    }
}
