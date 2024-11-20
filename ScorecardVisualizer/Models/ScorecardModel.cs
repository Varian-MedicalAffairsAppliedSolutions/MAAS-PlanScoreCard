using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Series;
using PlanScoreCard.API.Models.ScoreCard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ScorecardVisualizer.Models
{
    internal class ScorecardModel
    {
        private InternalTemplateModel _internalTemplateModel;
        private string _selectedStructure;

        public bool IsRead = false;

        public string Creator => _internalTemplateModel.Creator;
        public string DosePerFraction => $"{_internalTemplateModel.DosePerFraction} Gy";
        public string NumberOfFractions => _internalTemplateModel.NumberOfFractions.ToString();
        public string Site => _internalTemplateModel.Site;
        public string ModelName => _internalTemplateModel.TemplateName;
        public string TotalScore => StructurePlotInfos.Select(s => s.TotalPoints).Sum().ToString();

        public ObservableCollection<StructurePlotInfo> StructurePlotInfos = new ObservableCollection<StructurePlotInfo>();

        private List<string> _structureIds => _internalTemplateModel.ScoreTemplates.Select(s => s.Structure.TemplateStructureId).Distinct().OrderBy(i => i).ToList();

        public PlotModel Plot;


        public List<ScoreTemplateModel> SelectedStructureMetrics => _internalTemplateModel.ScoreTemplates.Where(s => s.Structure.TemplateStructureId == _selectedStructure).ToList();



        public ScorecardModel()
        {

        }

        private void LoadStructurePlotInfo()
        {
            StructurePlotInfos.Clear();

            Random rnd = new Random();

            foreach (string structure in _structureIds)
            {
                List<ScoreTemplateModel> scoreTemplates = _internalTemplateModel.ScoreTemplates.Where(s => s.Structure.TemplateStructureId == structure).ToList();

                List<double> maxScores = new List<double>();

                foreach (ScoreTemplateModel scoreTemplate in scoreTemplates)
                {
                    List<double> scores = new List<double>();
                    foreach (ScorePointInternalModel scorePoint in scoreTemplate.ScorePoints)
                    {
                        scores.Add(scorePoint.Score);
                    }
                    maxScores.Add(scores.Max());
                }

                double totalMaxScore = maxScores.Sum();

                StructurePlotInfos.Add(new StructurePlotInfo(structure, totalMaxScore, scoreTemplates, rnd));
            }

            LoadPlotModel();
        }

        public StructurePlotInfo LoadPlotModel(string structureToExplode = "")
        {
            _selectedStructure = structureToExplode;
            //OxyColor newColor = OxyColor.Parse("#F8F8FF"); 

            StructurePlotInfo selectedStructurePlotInfo = null;

            Plot = new PlotModel();

            dynamic series = new PieSeries
            {
                StrokeThickness = 1,
                AngleSpan = 360,
                StartAngle = 0,
                ExplodedDistance = 0.1,
                TickHorizontalLength = 5,
                TickRadialLength = 5,
                InsideLabelFormat = "",
                SelectionMode = SelectionMode.Single
            };

            foreach (var item in StructurePlotInfos)
            {
                if (item.StructureId == structureToExplode)
                {
                    item.IsSelected = true;
                    //newColor = item.WindowColor;
                    selectedStructurePlotInfo = item;
                    series.Slices.Add(new PieSlice(item.StructureId, item.TotalPoints) { Fill = item.Color, IsExploded = true });
                    continue;
                }
                series.Slices.Add(new PieSlice(item.StructureId, item.TotalPoints) { Fill = item.Color });
                item.IsSelected = false;
            }

            Plot.Series.Add(series);
            return selectedStructurePlotInfo;
        }

        public bool OpenScorecard(string filepath)
        {
            try
            {
                _internalTemplateModel = JsonConvert.DeserializeObject<InternalTemplateModel>(File.ReadAllText(filepath));
                LoadStructurePlotInfo();
                IsRead = true;
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }
}
