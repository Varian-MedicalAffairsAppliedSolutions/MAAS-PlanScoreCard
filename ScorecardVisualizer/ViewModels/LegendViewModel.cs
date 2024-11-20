using ScorecardVisualizer.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace ScorecardVisualizer.ViewModels
{
    public class LegendViewModel : ViewModelBase
    {
        public ObservableCollection<LegendItemViewModel> LegendItemViewModels { get; private set; }

        public void Update(ScorecardModel scorecardModel, StructurePlotInfo selectedStructurePlotInfo = null, ListView legendListView = null)
        {
            LegendItemViewModels = new ObservableCollection<LegendItemViewModel>(scorecardModel.StructurePlotInfos.Select(s => new LegendItemViewModel(s, scorecardModel)));
            OnPropertyChanged(nameof(LegendItemViewModels));

            if (selectedStructurePlotInfo != null)
            {
                LegendItemViewModel legendItemViewModelToScrollTo = LegendItemViewModels.FirstOrDefault(i => i.StructureId == selectedStructurePlotInfo.StructureId);
                legendListView.ScrollIntoView(legendItemViewModelToScrollTo);
            }
        }
    }
}
