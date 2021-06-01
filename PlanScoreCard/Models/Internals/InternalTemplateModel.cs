using System.Collections.Generic;

namespace PlanScoreCard.Models.Internals
{
    public class InternalTemplateModel
    {

        public string TemplateName;
        public string Site;
        public string Creator;
        public List<ScoreTemplateModel> ScoreTemplates;
        public InternalTemplateModel()
        {
            ScoreTemplates = new List<ScoreTemplateModel>();
        }
    }
}
