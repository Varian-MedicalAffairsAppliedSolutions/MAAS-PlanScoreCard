using OxyPlot;
using ScorecardVisualizer.Models;
using ScorecardVisualizer.Services.Commands;
using System.Windows.Input;

namespace ScorecardVisualizer.ViewModels
{
    public class LegendItemViewModel : ViewModelBase
    {
        private StructurePlotInfo _structurePlotInfo;

        public bool IsSelected => _structurePlotInfo.IsSelected;

        public string StructureId => _structurePlotInfo.StructureId;

        public double TotalPoints => _structurePlotInfo.TotalPoints;

        public OxyColor Color => _structurePlotInfo.Color;

        public OxyColor WindowColor => _structurePlotInfo.WindowColor;

        public string TotalScore { get; set; }

        public ICommand FocusOnStructureCommand { get; set; }

        public LegendItemViewModel(StructurePlotInfo structurePlotInfo, ScorecardModel scorecardModel)
        {
            _structurePlotInfo = structurePlotInfo;
            TotalScore = scorecardModel.TotalScore;

            FocusOnStructureCommand = new FocusOnStructureCommand(scorecardModel);
        }
    }
}
