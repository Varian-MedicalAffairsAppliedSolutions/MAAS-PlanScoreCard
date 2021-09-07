using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Events.Plugin;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels
{
    public class PluginPlotViewModel : BindableBase
    {
        private IEventAggregator EventAggregator;

        public string Feedbacks { get; private set; }

        private PlotModel plotData;
        public PlotModel PlotData
        {
            get { return plotData; }
            set { SetProperty(ref plotData, value); }
        }

        public PluginPlotViewModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<ConsoleUpdateEvent>().Subscribe(OnConsoleUpdate);
            EventAggregator.GetEvent<PlotUpdateEvent>().Subscribe(OnUpdatePlot);
            EventAggregator.GetEvent<PluginVisibilityEvent>().Subscribe(OnPluginRun);
        }

        public void SetPlotData(PlotModel plotData)
        {
            PlotData = plotData; 
        }

        private void OnPluginRun(bool obj)
        {
            // ConsoleOutput = String.Empty;
            PlotData.Series.Clear();
            PlotData.Axes.Clear();
            // bReloadAvailable = false;
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
            //ConsoleOutput += System.Environment.NewLine + obj;
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
