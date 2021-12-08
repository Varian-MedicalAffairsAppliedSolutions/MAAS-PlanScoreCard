using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class StructureDictionaryModel
    {

        // Idea: Use this as a flag for if something is an exclusion, that way we don't need to have multiple collections, this way you can filter the collection down to the exclusions.
        public bool isExclusion { get; set; }

        public string StructureID { get; set; }

        public List<string> StructureSynonyms { get; set; }

        public StructureDictionaryModel(string structureID, List<string> structureSynonyms)
        {
            StructureID = structureID;
            StructureSynonyms = structureSynonyms;
        }
    }
}
