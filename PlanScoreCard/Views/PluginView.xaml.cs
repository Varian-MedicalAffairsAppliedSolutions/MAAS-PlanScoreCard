using MahApps.Metro.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
        private bool disposed;
        private readonly Timer timer;
        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(string property)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        // ConsoleOutput Binding Proeperty
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

        // PlotModel Binding Proerty
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


        private string xAxisLabel;
        public string XAxisLabel
        {
            get { return xAxisLabel; }
            set 
            { 
                xAxisLabel = value;
                OnPropertyChanged();
            }
        }

        private string yAxisLabel;
        public string YAxisLabel
        {
            get { return yAxisLabel; }
            set
            {
                yAxisLabel = value;
                OnPropertyChanged();
            }
        }

        public PluginView()
        {
            ConsoleOutput = "";
            DataContext = this;

            InititatePlotModel();

            InitializeComponent();
        }

        private void InititatePlotModel()
        {

            // ** DEV NOTE: Are the axies titles alwaays constant? Do I update to change this string to add units based on their selected parameters?
            XAxisLabel = "Iteration";
            YAxisLabel = "Plan Score";

            PlotModel = new PlotModel();
            PlotModel.Axes.Add(new LinearAxis 
                                        { 
                                            Title = XAxisLabel, 
                                            Position = AxisPosition.Bottom,
                                            FontSize = 18,
                                            TitleFontWeight = 200
                                        });

            PlotModel.Axes.Add(new LinearAxis 
                                        { 
                                            Title = YAxisLabel, 
                                            Position = AxisPosition.Left,
                                            FontSize = 18,
                                            TitleFontWeight = 200
                                        });

            LineSeries series = new LineSeries();
            series.Color = OxyColor.FromArgb(255,16,161,211);
            series.StrokeThickness = 1.2;
            PlotModel.Series.Add(series);
        }

        public void UpdatePlot(PlotModel plotModel)
        {
            PlotModel = plotModel;

            PlotModel.InvalidatePlot(true);
            RaisePropertyChanged("PlotModel");
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

        internal void AddPlotPoint(double xval, double yval)
        {
            LineSeries series = (LineSeries)PlotModel.Series.First();
            series.Points.Add(new DataPoint(xval, yval));
            PlotModel.InvalidatePlot(true);
        }

        internal void SetXAxisLabel(string xAxisLabel)
        {
            XAxisLabel = xAxisLabel;
            PlotModel.InvalidatePlot(true);
        }

        internal void SetYAxisLabel(string yAxisLabel)
        {
            YAxisLabel = yAxisLabel;
            PlotModel.InvalidatePlot(true);
        }
    }
}
