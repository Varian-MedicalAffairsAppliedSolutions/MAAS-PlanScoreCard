using OxyPlot;
using PlanScoreCard.API.Models.ScoreCard;
using ScorecardVisualizer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ScorecardVisualizer.Models
{
    public class StructurePlotInfo
    {
        #region Properties
        public bool IsSelected { get; set; }
        public string StructureId { get; set; }
        public double TotalPoints { get; set; }
        public OxyColor BackgroundColor { get; set; }
        public OxyColor WindowColor { get; set; }
        public OxyColor SelectedBackgroundColor { get; set; }
        public List<MetricInfo> Metrics { get; set; }
        public string StructureAndPoints { get; set; }

        private OxyColor _color;
        public OxyColor Color => _color;
        #endregion

        #region Constructors
        public StructurePlotInfo(string structureId, double totalPoints, List<ScoreTemplateModel> scoreTemplates, Random rnd)
        {
            IsSelected = false;
            StructureId = structureId;
            TotalPoints = totalPoints;
            StructureAndPoints = $"{structureId} ({totalPoints} points)";

            GetColorMatch(structureId, rnd);

            Metrics = new List<MetricInfo>();

            // Make lighter background color
            BackgroundColor = Lighten(Color, 5);
            WindowColor = Lighten(Color, 7);

            for (int i = 0; i < scoreTemplates.Count; i++)
            {
                // Same color as background
                Metrics.Add(new MetricInfo(scoreTemplates[i], TotalPoints, BackgroundColor));

            }
        }
        #endregion

        #region Methods
        private OxyColor Lighten(OxyColor baseColor, double factor)
        {
            byte r = (byte)(baseColor.R + (255 - Color.R) * (0.1 * factor));
            byte g = (byte)(baseColor.G + (255 - Color.G) * (0.1 * factor));
            byte b = (byte)(baseColor.B + (255 - Color.B) * (0.1 * factor));

            return OxyColor.FromRgb(r, g, b);
        }

        private void GetColorMatch(string structureId, Random rnd)
        {
            bool colorSet = false;

            // Check for direct match to structure dictionary
            if (Dictionaries.StructureDictionary.Any(s => s.Structure == structureId))
            {
                _color = Dictionaries.StructureDictionary.FirstOrDefault(s => s.Structure == structureId).GetOxyColor();
                colorSet = true;
            }

            // Check for synonyms
            else if (Dictionaries.StructureDictionary.Any(s => s.Synonyms.Contains(structureId)))
            {
                _color = Dictionaries.StructureDictionary.FirstOrDefault(s => s.Synonyms.Contains(structureId)).GetOxyColor();
                colorSet = true;
            }

            // Check Regex
            else 
            {
                foreach (var s in Dictionaries.StructureDictionary)
                {
                    if (s.RegEx != null)
                        if (Regex.IsMatch(structureId, s.RegEx))
                        {
                            _color = s.GetOxyColor();
                            colorSet = true;
                            break;
                        }
                }
            }

            // No match, generate pseudo random color
            if (!colorSet)
            {
                byte[] values = new byte[3];
                rnd.NextBytes(values);
                values[rnd.Next(3)] += 128;

                _color = OxyColor.FromRgb(values[0], values[1], values[2]);
            }
        }
        #endregion
    }
}
