using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Events;
using PlanScoreCard.Events.Plugin;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PlanScoreCard.ViewModels
{
    public class PluginViewModel : BindableBase
    {
        private string _consoleOutput;
        private IEventAggregator _eventAggregator;
        private ViewLauncherService ViewLauncherService;
        private PluginViewService PluginViewService;

        private readonly IEventAggregator EventAggregator;
        private readonly IEventAggregator ViewEventAggregator;
        private Dispatcher ViewDispatcher;

        public string ConsoleOutput
        {
            get { return _consoleOutput; }
            set { SetProperty(ref _consoleOutput, value); }
        }
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
        public PluginViewModel(IEventAggregator eventAggregator, ViewLauncherService viewLauncherService , PluginViewService pluginViewService)
        {
            _eventAggregator = eventAggregator;
            ViewLauncherService = viewLauncherService;
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
        private void OnUpdatePlot(string obj)
        {
            if (obj.StartsWith("Series"))
            {
                if (obj.Split('_').ElementAt(1) == "X")
                {
                    PlotData.Axes.Add(new LinearAxis { Title = obj.Split('_').Last(), Position = AxisPosition.Bottom });
                }
                else if (obj.Split('_').ElementAt(1) == "Y")
                {
                    PlotData.Axes.Add(new LinearAxis { Title = obj.Split('_').Last(), Position = AxisPosition.Left });
                }
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

            PluginViewService.UpdatePlot(PlotData);
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
            series.Points.Clear();
            foreach (var point in PlotSeries.FirstOrDefault(x => x.Title == title).DataPoints.OrderBy(x => x.Item1))
            {
                series.Points.Add(new DataPoint(point.Item1, point.Item2));
            }
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
