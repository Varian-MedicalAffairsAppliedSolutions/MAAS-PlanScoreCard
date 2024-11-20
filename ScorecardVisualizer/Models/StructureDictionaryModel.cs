using Newtonsoft.Json;
using OxyPlot;
using System.Collections.Generic;

namespace ScorecardVisualizer.Models
{
    public class StructureDictionaryModel
    {
        [JsonProperty("Structure")]
        public string Structure;

        [JsonProperty("RegEx")]
        public string RegEx = null;

        [JsonProperty("Color")]
        public byte[] Color = null;

        [JsonProperty("Synonyms")]
        public List<string> Synonyms = new List<string> { "" };

        [JsonProperty("SubStructures")]
        public List<string> SubStructures = null;

        public OxyColor GetOxyColor()
        {
            return OxyColor.FromRgb(Color[0], Color[1], Color[2]);
        }
    }
}
