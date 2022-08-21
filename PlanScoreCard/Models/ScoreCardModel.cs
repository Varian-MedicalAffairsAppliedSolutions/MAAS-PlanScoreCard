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
            //set color to white if no color determined.
            if (ScoreMetrics != null)
            {
                foreach (var sm in ScoreMetrics)
                {
                    //at least one color must be set, but another must not be set. Then set to white. 
                    if (sm.ScorePoints.Any(sp => !sp.Colors.Any()) && sm.ScorePoints.Any(sp => sp.Colors.Any()))
                    {
                        foreach (var sp in sm.ScorePoints.Where(sp => !sp.Colors.Any()))
                        {
                            sp.Colors = new List<double> { 255, 255, 255 };
                            sp.Label = $"[{sp.Score}]";
                            sp.ColorValue = sp.Score;
                        }
                    }
                }
            }
            DosePerFraction = dosePerFraction;
            NumberOfFractions = numberOfFractions;

            if (ScoreMetrics == null)
            {
                ScoreMetrics = new List<ScoreTemplateModel>();
            }
        }

    }
}
