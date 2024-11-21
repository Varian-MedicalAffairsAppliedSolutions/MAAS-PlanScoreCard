using Newtonsoft.Json;
using OxyPlot;
using ScorecardVisualizer.Models;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace ScorecardVisualizer.Services
{
    public static class Dictionaries
    {
        public static bool IsRead = false;

        public static List<StructureDictionaryModel> StructureDictionary = null;

        public static void ReadDictionaries()
        {
            StructureDictionary = JsonConvert.DeserializeObject<List<StructureDictionaryModel>>(File.ReadAllText(ConfigurationManager.AppSettings["StructureDictionaryPath"]));
            IsRead = true;
        }

        public static Dictionary<string, string> MetricPrefix = new Dictionary<string, string>()
        {
            { "MinDose", "Dmin"},
            { "MaxDose", "Dmax"},
            { "MeanDose","Dmean" },
            { "DoseAtVolume","D" },
            { "VolumeAtDose","V"},
            { "Volume","V"},
            { "VolumeOfRegret","VoR"},
            { "ConformationNumber","CN"},
            { "HomogeneityIndex","HI"},
            { "ConformityIndex","CI"},
            { "InhomogeneityIndex","iHI"},
            { "ModifiedGradientIndex","MGI"},
            { "DoseAtSubVolume","DsV"},
            { "Undefine","U"}
        };
    }
}
