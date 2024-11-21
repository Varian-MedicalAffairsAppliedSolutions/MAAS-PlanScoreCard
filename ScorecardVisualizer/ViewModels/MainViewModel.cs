using OxyPlot;
using ScorecardVisualizer.Models;
using ScorecardVisualizer.Services;
using ScorecardVisualizer.Services.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScorecardVisualizer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Properties
        private ScorecardModel _scorecardModel;
        #endregion

        #region Commands
        public ICommand LaunchOpenScorecardPromptCommand { get; set; }

        public ICommand TestCommand { get; set; }
        #endregion

        #region Properties
        public PlotModel Plot => _scorecardModel.IsRead ? _scorecardModel.Plot : null;
        public string Author => _scorecardModel.IsRead ? _scorecardModel.Creator : "";
        public string DosePerFraction => _scorecardModel.IsRead ? _scorecardModel.DosePerFraction : "";
        public string NumberOfFractions => _scorecardModel.IsRead ? _scorecardModel.NumberOfFractions : "";
        public string TotalScore => _scorecardModel.IsRead ? _scorecardModel.TotalScore : "";
        public string Site => _scorecardModel.IsRead ? _scorecardModel.Site : "";
        public string ModelName => _scorecardModel.IsRead ? _scorecardModel.ModelName : "No Scorecard Selected";
        public OxyColor WindowBackground { get; set; }
        public ObservableCollection<MetricGraphViewModel> SelectedStructureMetricGraphs { get; set; }
        public LegendViewModel LegendViewModel { get; set; }
        public MetricDisplayViewModel MetricDisplayViewModel { get; set; }

        public string IsLaunchedFromScorecard { get; set; }
        #endregion

        #region Constructors
        public MainViewModel()
        {
            if (!Dictionaries.IsRead)
                Dictionaries.ReadDictionaries();

            IsLaunchedFromScorecard = "Visible";

            WindowBackground = OxyColor.Parse("#F8F8FF");

            _scorecardModel = new ScorecardModel();

            LaunchOpenScorecardPromptCommand = new LaunchOpenScorecardPromptCommand(_scorecardModel);
            TestCommand = new TestCommand();

            LegendViewModel = new LegendViewModel();
            MetricDisplayViewModel = new MetricDisplayViewModel();

            Messenger.UpdateScorecard += Messenger_UpdateScorecard;
            Messenger.UpdatePlot += Messenger_UpdatePlot;
            Messenger.SelectStructure += Messenger_SelectStructure;
            Messenger.LaunchFromPlanScorecard += Messenger_LaunchFromPlanScorecard;
        }

        #endregion

        #region Messenger   
        private void Messenger_UpdateScorecard(object sender, System.EventArgs e)
        {
            OnPropertyChanged(nameof(Author));
            OnPropertyChanged(nameof(DosePerFraction));
            OnPropertyChanged(nameof(NumberOfFractions));
            OnPropertyChanged(nameof(TotalScore));
            OnPropertyChanged(nameof(Site));
            OnPropertyChanged(nameof(ModelName));
            OnPropertyChanged(nameof(Plot));

            SelectedStructureMetricGraphs = new ObservableCollection<MetricGraphViewModel>();
            OnPropertyChanged(nameof(SelectedStructureMetricGraphs));

            LegendViewModel.Update(_scorecardModel);
            MetricDisplayViewModel.Update(_scorecardModel);
        }

        private void Messenger_UpdatePlot(object sender, System.EventArgs e)
        {
            OnPropertyChanged(nameof(Plot));
        }

        private void Messenger_SelectStructure(object sender, System.EventArgs e)
        {
            SelectedStructureMetricGraphs = new ObservableCollection<MetricGraphViewModel>(_scorecardModel.SelectedStructureMetrics.Select(s => new MetricGraphViewModel(s)));

            var parameters = sender as Tuple<StructurePlotInfo, ListView, ListView>;

            WindowBackground = parameters.Item1.WindowColor;

            MetricDisplayViewModel.Update(_scorecardModel, parameters.Item1, parameters.Item2);
            LegendViewModel.Update(_scorecardModel, parameters.Item1, parameters.Item3);

            OnPropertyChanged(nameof(SelectedStructureMetricGraphs));
            OnPropertyChanged(nameof(WindowBackground));
        }

        private void Messenger_LaunchFromPlanScorecard(object sender, EventArgs e)
        {
            _scorecardModel = (ScorecardModel)sender;
            IsLaunchedFromScorecard = "Hidden";
            OnPropertyChanged(nameof(IsLaunchedFromScorecard));
            Messenger_UpdateScorecard(null, null);
        }

        #endregion
    }
}
