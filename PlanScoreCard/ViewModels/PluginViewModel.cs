using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Events;
using PlanScoreCard.Events.Plugin;
using PlanScoreCard.Events.Plugins;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PlanScoreCard.ViewModels
{
    public class PluginViewModel : BindableBase, INotifyPropertyChanged
    {
        private IEventAggregator _eventAggregator;
        private PluginViewService PluginViewService;

        private readonly IEventAggregator EventAggregator;
        private readonly IEventAggregator ViewEventAggregator;
        private Dispatcher ViewDispatcher;

        private bool _breloadAvailable;

        public bool bReloadAvailable
        {
            get { return _breloadAvailable; }
            set { SetProperty(ref _breloadAvailable, value); }
        }
        private bool _bOptimizeAvailable;

        public bool bOptimizeAvailable
        {
            get { return _bOptimizeAvailable; }
            set { SetProperty(ref _bOptimizeAvailable, value); }
        }
        private bool _bFinalizeAvailable;

        public bool bFinalizeAvailable
        {
            get { return _bFinalizeAvailable; }
            set { SetProperty(ref _bFinalizeAvailable, value); }
        }

        private UserControl pluginConsoleUserControl;

        public UserControl PluginConsoleUserControl
        {
            get { return pluginConsoleUserControl; }
            set { SetProperty<UserControl>(ref pluginConsoleUserControl, value); }
        }

        private UserControl pluginPlotUserControl;

        public UserControl PluginPlotUserControl
        {
            get { return pluginPlotUserControl; }
            set { SetProperty<UserControl>(ref pluginPlotUserControl, value); }
        }

        public DelegateCommand ReloadAppCommand { get; private set; }
        public DelegateCommand OptimizeCommand { get; private set; }
        public DelegateCommand FinalizeCommand { get; private set; }
        public PlotModel PlotData { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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


        public PluginViewModel(IEventAggregator eventAggregator, PluginViewService pluginViewService)
        {
            _eventAggregator = eventAggregator;
            //Feedbacks = new List<string>();
            PlotData = new PlotModel() { LegendPosition = LegendPosition.RightTop };
            PlotSeries = new List<PlotSeriesData>();
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Subscribe(OnConsoleUpdate);
            _eventAggregator.GetEvent<PlotUpdateEvent>().Subscribe(OnUpdatePlot);
            _eventAggregator.GetEvent<PluginVisibilityEvent>().Subscribe(OnPluginRun);
            _eventAggregator.GetEvent<ShowPluginViewEvent>().Subscribe(ShowPluginView);
            ReloadAppCommand = new DelegateCommand(OnReloadApp);
            OptimizeCommand = new DelegateCommand(OnOptimize);
            FinalizeCommand = new DelegateCommand(OnFinalize);

            ViewEventAggregator = new EventAggregator();

            PluginViewService = pluginViewService;
            YAxisLabel = "Score";
            XAxisLabel = "Interation";
        }

        private void ShowPluginView()
        {
            PluginViewService.ShowPluginView();
            PluginViewService.SendToFront();
        }

        private void OnFinalize()
        {
            //_eventAggregator.GetEvent<FinalizePluginEvent>().Publish(null);
        }

        private void OnOptimize()
        {
            //_eventAggregator.GetEvent<OptimizePluginEvent>().Publish(null);
        }

        private void OnPluginRun(bool obj)
        {
            ConsoleOutput = String.Empty;
            PlotData.Series.Clear();
            PlotData.Axes.Clear();
            bReloadAvailable = false;
            //ReloadAppCommand.RaiseCanExecuteChanged();
        }

        List<PlotSeriesData> PlotSeries;
        private void OnUpdatePlot(List<PlanScoreModel> obj)
        {
            foreach (var score in obj)
            {
                string title = "Total Score";
                if (!PlotSeries.Any(x => x.Title == title))
                {
                    GeneratePlotSeries(title);
                    PlotSeries.Add(new PlotSeriesData
                    {
                        Title = title
                    });
                }
                double iteration = PlotSeries.FirstOrDefault(x => x.Title == title).DataPoints.Count();
                double value = -1;
                //check to see the value of "plotNegative" in the config, but also check that the sum of all scorevalues is >0. 
                if (ConfigurationManager.AppSettings["PlotNegative"] == "true" || obj.Sum(psm => psm.ScoreValues.First().Score) >= 0)
                {
                    value = obj.Sum(psm => psm.ScoreValues.First().Score);
                }
                PlotSeries.FirstOrDefault(x => x.Title == title).DataPoints.Add(
                           new Tuple<double, double>(iteration,
                           value));
                ResetPlotSeries(title, iteration, value);
            }


            /*if (obj.StartsWith("Series"))
            {
                if (obj.Split('_').ElementAt(1) == "X")
                {
                    //PlotData.Axes.Add(new LinearAxis { Title = obj.Split('_').Last(), Position = AxisPosition.Bottom });
                    PluginViewService.UpdateXAxisLebel(obj.Split('_').Last());
                }
                else if (obj.Split('_').ElementAt(1) == "Y")
                {
                    //PlotData.Axes.Add(new LinearAxis { Title = obj.Split('_').Last(), Position = AxisPosition.Left });
                    PluginViewService.UpdateYAxisLebel(obj.Split('_').Last());
                }
                PluginViewService.UpdatePlot(PlotData);
            }
            else if (obj.StartsWith("PlotPoint"))
            {
                //PlotSeries[obj.Split('<').Last().Split(';').First()].Add(new Tuple<double, double>(
                //       Convert.ToDouble(obj.Split('<').Last().Split(';').ElementAt(1)),
                //       Convert.ToDouble(obj.Split(';').Last().TrimEnd('>'))));

                var title = obj.Split('<').Last().Split(';').First();
                //check if values can be converted to double.
                double norm_value = 0.0;
                double score_value = 0.0;
                var provider = new CultureInfo("en-US");
                if (obj.Split('<').Last().Split(';').Count() > 1 &&
                    Double.TryParse(obj.Split('<').Last().Split(';').ElementAt(1), NumberStyles.Float, provider, out norm_value) &&
                    Double.TryParse(obj.Split(';').Last().TrimEnd('>'), NumberStyles.Float, provider, out score_value))
                {
                    if (!PlotSeries.Any(x => x.Title == title))
                    {
                        GeneratePlotSeries(title);
                        PlotSeries.Add(new PlotSeriesData
                        {
                            Title = title
                        });

                        //PlotSeries.First(x => x.Title == title).DataPoints.Add(new Tuple<double, double>(
                        //    norm_value, score_value));

                        //PlotSeries.FirstOrDefault(x => x.Title == obj.Split('<').Last().Split(';').First()).DataPoints.Add(
                        //    new Tuple<double, double>(Convert.ToDouble(),
                        //   Convert.ToDouble()));                       

                    }
                    //else
                    //{


                    //}
                    PlotSeries.FirstOrDefault(x => x.Title == title).DataPoints.Add(
                         new Tuple<double, double>(norm_value, score_value));
                    ResetPlotSeries(title, norm_value, score_value);
                }
            }
            else if (obj.StartsWith("Metric"))
            {
                //_eventAggregator.GetEvent<UpdateMetricDuringOptimizationEvent>().Publish(obj);
            }
            */
        }

        private void GeneratePlotSeries(string obj)
        {
            var series = new LineSeries()
            {
                Title = obj
            };
            //series.Points.Add(new DataPoint(
            //     Convert.ToDouble(obj.Split('<').Last().Split(';').ElementAt(1)),
            //           Convert.ToDouble(obj.Split(';').Last().TrimEnd('>'))));
            PlotData.Series.Add(series);
            // PlotData.InvalidatePlot(true);
        }

        private void ResetPlotSeries(string title, double xval, double yval)
        {
            LineSeries series = PlotData.Series.First(x => x.Title == title) as LineSeries;
            series.Points.Add(new DataPoint(xval, yval));
            PluginViewService.AddPlotPoint(xval, yval);

            //series.Points.Clear();
            //foreach (var point in PlotSeries.FirstOrDefault(x => x.Title == title).DataPoints.OrderBy(x => x.Item1))
            //{
            //    series.Points.Add(new DataPoint(point.Item1, point.Item2));
            //}
            PlotData.InvalidatePlot(true);
        }

        private void OnReloadApp()
        {
            //_eventAggregator.GetEvent<ResetApplicationEvent>().Publish(Feedbacks);
            //Feedbacks.Clear();
        }
        public string Feedbacks { get; private set; }

        private void OnConsoleUpdate(string obj)
        {
            if (obj.Contains("Activate"))
            {
                Feedbacks = obj;
            }
            //ReloadAppCommand.RaiseCanExecuteChanged();
            //if (obj.Contains("Finished Initialize")) 
            //{ 
            //    bOptimizeAvailable = true; 
            //    if(ConfigurationManager.AppSettings["OptMode"] == "Auto")
            //    {
            //        OnOptimize();
            //    }
            //}
            //if(obj.Contains("Finished Optimize")) 
            //{ 
            //    bFinalizeAvailable = true; 
            //    if(ConfigurationManager.AppSettings["OptMode"] == "Auto")
            //    {
            //        OnFinalize();
            //    }
            //}
            //if (obj.Contains("Activate")||obj.Contains("Finished Finalize")) 
            //{
            //    Thread.Sleep(8000);
            //    bReloadAvailable = true; 
            //}
            ConsoleOutput += System.Environment.NewLine + obj;
            PluginViewService.UpdateConsoleOutput(ConsoleOutput);
        }
    }
    public class PlotSeriesData
    {
        public string Title { get; set; }
        public List<Tuple<double, double>> DataPoints { get; set; }
        public PlotSeriesData()
        {
            DataPoints = new List<Tuple<double, double>>();
        }
    }
}
