using MahApps.Metro.Controls;
using OxyPlot;
using PlanScoreCard.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlanScoreCard.Views
{
    /// <summary>
    /// Interaction logic for PluginView.xaml
    /// </summary>
    public partial class PluginView : MetroWindow , INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private string consoleOutput;
        public string ConsoleOutput
        {
            get { return consoleOutput; }
            set 
            { 
                consoleOutput = value;
                OnPropertyChanged();
            }
        }

        private PlotModel plotModel;

        public PlotModel PlotModel
        {
            get { return plotModel; }
            set 
            { 
                plotModel = value;
                OnPropertyChanged();
            }
        }


        public PluginView()
        {
            PlotModel = new PlotModel();
            ConsoleOutput = "";
            DataContext = this;
            InitializeComponent();
        }


        public void UpdatePlot(PlotModel plotModel)
        {
            PlotModel = plotModel;
            PlotModel.InvalidatePlot(true);
        }

        public void UpdateConsoleOutput(string consoleOutput)
        {
            ConsoleOutput = consoleOutput;
        }

        public void SendToFront()
        {
            Topmost = true;
            Topmost = false;
        }


    }
}
