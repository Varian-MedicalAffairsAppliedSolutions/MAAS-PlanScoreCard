using ScorecardVisualizer.ViewModels;
using System.Windows;

namespace ScorecardVisualizer.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private MainViewModel MainViewModel;

        public MainView()
        {
            InitializeComponent();
        }

        public MainView(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            DataContext = MainViewModel;
            InitializeComponent();
        }
    }
}
