using ScorecardVisualizer.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace ScorecardVisualizer.ViewModels
{
    public class MetricDisplayViewModel : ViewModelBase
    {
        public ObservableCollection<MetricDisplayItemViewModel> MetricDisplayItemViewModels { get; private set; }

        public void Update(ScorecardModel scorecardModel, StructurePlotInfo selectedStructurePlotInfo = null, ListView metricDisplayListView = null)
        {
            MetricDisplayItemViewModels = new ObservableCollection<MetricDisplayItemViewModel>(scorecardModel.StructurePlotInfos.Select(s => new MetricDisplayItemViewModel(s, scorecardModel)));
            OnPropertyChanged(nameof(MetricDisplayItemViewModels));

            if (selectedStructurePlotInfo != null)
            {
                MetricDisplayItemViewModel metricDisplayItemViewModelToScrollTo = MetricDisplayItemViewModels.FirstOrDefault(i => i.StructureId == selectedStructurePlotInfo.StructureId);
                metricDisplayListView.ScrollIntoView(metricDisplayItemViewModelToScrollTo);
            }
        }
    }
}
