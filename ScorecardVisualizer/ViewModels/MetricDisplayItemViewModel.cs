using OxyPlot;
using ScorecardVisualizer.Models;
using ScorecardVisualizer.Services.Commands;
using System.Collections.Generic;
using System.Windows.Input;

namespace ScorecardVisualizer.ViewModels
{
    internal class MetricDisplayItemViewModel
    {
        private StructurePlotInfo _structurePlotInfo;

        public ICommand FocusOnStructureCommand { get; set; }

        public string StructureId => _structurePlotInfo.StructureId;

        public string StructureAndPoints => _structurePlotInfo.StructureAndPoints;

        public List<MetricInfo> Metrics => _structurePlotInfo.Metrics;

        public OxyColor BackgroundColor => _structurePlotInfo.BackgroundColor;

        public OxyColor Color => _structurePlotInfo.Color;


        public MetricDisplayItemViewModel(StructurePlotInfo structurePlotInfo, ScorecardModel scorecardModel)
        {
            _structurePlotInfo = structurePlotInfo;

            FocusOnStructureCommand = new FocusOnStructureCommand(scorecardModel);
        }
    }
}
