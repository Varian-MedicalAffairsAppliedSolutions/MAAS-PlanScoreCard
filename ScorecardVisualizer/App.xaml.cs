using ScorecardVisualizer.Services;
using ScorecardVisualizer.ViewModels;
using ScorecardVisualizer.Views;
using System.Windows;

namespace ScorecardVisualizer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            Dictionaries.ReadDictionaries();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainView(false)
            {
                DataContext = new MainViewModel()
            };
            MainWindow.ShowDialog();

        }
    }
}
