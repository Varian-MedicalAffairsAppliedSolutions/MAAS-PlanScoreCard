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
        public bool IsSelected { get; set; }
        public string StructureId { get; set; }
        public double TotalPoints { get; set; }

        private OxyColor _color;
        public OxyColor Color => _color;

        public OxyColor BackgroundColor { get; set; }
        public OxyColor WindowColor { get; set; }
        public OxyColor SelectedBackgroundColor { get; set; }

        public List<MetricInfo> Metrics { get; set; }

        public string StructureAndPoints { get; set; }

        public StructurePlotInfo(string structureId, double totalPoints, List<ScoreTemplateModel> scoreTemplates, Random rnd)
        {
            IsSelected = false;
            StructureId = structureId;
            TotalPoints = totalPoints;
            StructureAndPoints = $"{structureId} ({totalPoints} points)";

            // Find if structureId either
            // 1. direct match to Structure
            // 2. exists in Synonyms
            // 3. satisfies RegEx

            bool colorSet = false;

            if (Dictionaries.StructureDictionary.Any(s => s.Structure == structureId))
            {
                _color = Dictionaries.StructureDictionary.FirstOrDefault(s => s.Structure == structureId).GetOxyColor();
                colorSet = true;
            }
            else if (Dictionaries.StructureDictionary.Any(s => s.Synonyms.Contains(structureId)))
            {
                _color = Dictionaries.StructureDictionary.FirstOrDefault(s => s.Synonyms.Contains(structureId)).GetOxyColor();
                colorSet = true;
            }
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

            if (!colorSet)
            {
                byte[] values = new byte[3];
                rnd.NextBytes(values);
                values[rnd.Next(3)] += 128;

                _color = OxyColor.FromRgb(values[0], values[1], values[2]);
            }

            Metrics = new List<MetricInfo>();

            // Make lighter background color
            BackgroundColor = Lighten(Color, 5);
            WindowColor = Lighten(Color, 7);

            // From the number of score templates I want to create that many colors that are slight variations of _color established above
            //List<OxyColor> generatedColors = GenerateColors(Color, scoreTemplates.Count());

            for (int i = 0; i < scoreTemplates.Count; i++)
            {
                // Faded colors
                //Metrics.Add(new MetricInfo(scoreTemplates[i], TotalPoints, generatedColors[i]));

                // Same color as background
                Metrics.Add(new MetricInfo(scoreTemplates[i], TotalPoints, BackgroundColor));

            }
        }

        private List<OxyColor> GenerateColors(OxyColor baseColor, int count)
        {
            List<OxyColor> colors = new List<OxyColor>();

            for (int i = 1; i < count + 1; i++)
                colors.Add(Lighten(baseColor, i));

            return colors;
        }

        private OxyColor Lighten(OxyColor baseColor, double factor)
        {
            byte r = (byte)(baseColor.R + (255 - Color.R) * (0.1 * factor));
            byte g = (byte)(baseColor.G + (255 - Color.G) * (0.1 * factor));
            byte b = (byte)(baseColor.B + (255 - Color.B) * (0.1 * factor));

            return OxyColor.FromRgb(r, g, b);
        }
    }
}
