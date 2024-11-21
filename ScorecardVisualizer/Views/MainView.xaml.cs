using ScorecardVisualizer.Services;
using ScorecardVisualizer.ViewModels;
using System.Web.UI.WebControls;
using System.Windows;

namespace ScorecardVisualizer.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private bool _fromScorecard;
        private MainViewModel MainViewModel;

        public MainView(bool fromScorecard = true)
        {
            _fromScorecard = fromScorecard;
            if (_fromScorecard)
            {
                MainViewModel = new MainViewModel();
                DataContext = MainViewModel;
            }

            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_fromScorecard)
                return;

            Messenger.UpdateScorecard -= MainViewModel.Messenger_UpdateScorecard;
            Messenger.UpdatePlot -= MainViewModel.Messenger_UpdatePlot;
            Messenger.SelectStructure -= MainViewModel.Messenger_SelectStructure;
            Messenger.LaunchFromPlanScorecard -= MainViewModel.Messenger_LaunchFromPlanScorecard;
        }
    }
}
