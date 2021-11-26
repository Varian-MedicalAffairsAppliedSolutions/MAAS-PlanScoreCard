using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class StructureDictionaryModel
    {
        public string StructureID { get; set; }

        public List<string> StructureSynonyms { get; set; }

        public StructureDictionaryModel(string structureID, List<string> structureSynonyms)
        {
            StructureID = structureID;
            StructureSynonyms = structureSynonyms;
        }
    }
}
