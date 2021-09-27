using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Events;
using PlanScoreCard.Services;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace PlanScoreCard.Models
{
    public class ScoreMetricModel : BindableBase, INotifyPropertyChanged
    {
        public IEventAggregator EventAggregator;

        // Metric Type
        public MetricTypeEnum MetricType { get; set; }

        // MetricName
        public string MetricText { get; set; }

        // Flag for the Rank Event of the ID - This is Gross.. Needs to Die
        public bool CanReorder;

        // ID (Rank)
        private int _id;

        public int Id
        {
            get { return _id; }
            set
            {
                int oldValue = _id;
                int newValue = value;

                if (EventAggregator != null && oldValue != newValue && CanReorder)
                {
                    Dictionary<int, int> rankChange = new Dictionary<int, int>();
                    rankChange.Add(oldValue, newValue);
                    EventAggregator.GetEvent<MetricRankChangedEvent>().Publish(rankChange);
                }

                SetProperty(ref _id, value);
            }
        }

        // Metric Structure
        private StructureModel structure;

        public StructureModel Structure
        {
            get { return structure; }
            set
            {
                value = Structures.FirstOrDefault(s => s.StructureId == value.StructureId);
                SetProperty(ref structure, value);
            }
        }

        // Available Structures
        public ObservableCollection<StructureModel> Structures { get; set; }

        // ScorePoints
        public ObservableCollection<ScorePointModel> ScorePoints { get; set; }

        // ScorePoint Plot
        public ViewResolvingPlotModel ScoreMetricPlotModel { get; set; }

        // Metric Parameters
        public string InputValue { get; internal set; }
        public string InputUnit { get; internal set; }
        public string OutputUnit { get; internal set; }
        public string HI_Hi { get; internal set; }
        public string HI_Lo { get; internal set; }
        public string HI_Target { get; internal set; }

        // Metric Level Settings - What is this?
        public Dictionary<string, double> ScoreMetricLevelSettings { get; set; }

        public ScoreMetricModel(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            ScorePoints = new ObservableCollection<ScorePointModel>();
            Structures = new ObservableCollection<StructureModel>();
            ScoreMetricLevelSettings = new Dictionary<string, double>();
            CanReorder = true;

            InititatePlot();
        }

        public void InititatePlot()
        {
            SetEvents();
            ScoreMetricPlotModel = new ViewResolvingPlotModel();
            SetPlotProperties(MetricType);
        }

        private void SetEvents()
        {
            EventAggregator.GetEvent<AddScorePointEvent>().Subscribe(OnAddPlotScorePoint);
            EventAggregator.GetEvent<DeleteScorePointEvent>().Subscribe(OnDeleteScorePoint);
            EventAggregator.GetEvent<PointUpEvent>().Subscribe(OnPointUp);
            EventAggregator.GetEvent<PointDownEvent>().Subscribe(OnPointDown);
            EventAggregator.GetEvent<VariationCheckedEvent>().Subscribe(OnVariationChanged);
            EventAggregator.GetEvent<ColorUpdateEvent>().Subscribe(OnColorUpdate);
        }

        private void SetPlotProperties(MetricTypeEnum metricType)
        {
            ScoreMetricPlotModel.Title = PlanScorePlottingServices.GetPlotTitle(metricType, this);// $"Score for Dose at {doseAtVolumeViewModel.Volume}{doseAtVolumeViewModel.SelectedVolumeUnit}";
            ScoreMetricPlotModel.LegendPlacement = LegendPlacement.Outside;
            ScoreMetricPlotModel.LegendPosition = LegendPosition.RightTop;
            ScoreMetricPlotModel.Axes.Add(new LinearAxis
            {
                Title = PlanScorePlottingServices.GetPlotXAxisTitle(metricType, this),// $"Dose [{doseAtVolumeViewModel.SelectedDoseUnit}]",
                Position = AxisPosition.Bottom,
            });
            ScoreMetricPlotModel.Axes.Add(new LinearAxis
            {
                Title = $"Score",
                Position = AxisPosition.Left
            });
        }

        private void OnAddPlotScorePoint(int obj)
        {
            SetVariation();//set the visibilty status again for when the score changes.
            if (Id == obj)
            {
                ScoreMetricPlotModel.Series.Clear();
                //break scorepoints into 2 groups, before and after the variation.
                //this one sets marker color.
                foreach (ScorePointModel spoint in ScorePoints.OrderBy(x => x.PointX))
                {
                    var ScorePointSeries = new LineSeries
                    {
                        //Color = OxyColors.Orange,
                        Color = GetColorFromMetric(spoint),
                        MarkerType = MarkerType.Diamond,
                        //MarkerFill = GetColorFromMetric(spoint),
                        MarkerSize = 8
                    };
                    //add to the plot
                    ScorePointSeries.Points.Add(new DataPoint(Convert.ToDouble(spoint.PointX), spoint.Score));
                    ScoreMetricPlotModel.Series.Add(ScorePointSeries);
                }
                if (ScorePoints.Any(x => x.bMetricChecked))
                {
                    var PointSeriesAbove = new LineSeries
                    {
                        Color = OxyColors.Green
                    };
                    foreach (var spoint in ScorePoints.Where(x => x.Score >= ScorePoints.SingleOrDefault(y => y.bMetricChecked).Score).OrderBy(x => x.PointX))
                    {

                        //add to the plot
                        PointSeriesAbove.Points.Add(new DataPoint(Convert.ToDouble(spoint.PointX), spoint.Score));
                    }
                    ScoreMetricPlotModel.Series.Add(PointSeriesAbove);

                    var PointSeriesBelow = new LineSeries
                    {
                        Color = OxyColors.Yellow
                    };
                    foreach (var spoint in ScorePoints.Where(x => x.Score <= ScorePoints.SingleOrDefault(y => y.bMetricChecked).Score).OrderBy(x => x.PointX))
                    {
                        //add to the plot
                        PointSeriesBelow.Points.Add(new DataPoint(Convert.ToDouble(spoint.PointX), spoint.Score));
                    }
                    ScoreMetricPlotModel.Series.Add(PointSeriesBelow);
                }
                else
                {
                    //make it all green if there is no variation.
                    var PointSeriesAllGreen = new LineSeries
                    {
                        Color = OxyColors.Green
                    };
                    foreach (var spoint in ScorePoints.OrderBy(x => x.PointX))
                    {
                        //add to the plot
                        PointSeriesAllGreen.Points.Add(new DataPoint(Convert.ToDouble(spoint.PointX), spoint.Score));
                    }
                    ScoreMetricPlotModel.Series.Add(PointSeriesAllGreen);
                }
                ScoreMetricPlotModel.InvalidatePlot(true);
            }
        }

        private void SetVariation()
        {
            if (ScorePoints.Any(x => x.bMetricChecked))
            {
                ScorePoints.FirstOrDefault(x => x.bMetricChecked).bMidMetric = true;
                return;//don't set the enableness if a metric is already checked.
            }
            foreach (ScorePointModel metric in ScorePoints)
            {
                if (!(metric.Score == ScorePoints.Max(x => x.Score) || metric.Score == ScorePoints.Min(x => x.Score)))
                {
                    metric.bMidMetric = true;
                }
                else
                {
                    metric.bMidMetric = false;
                }
            }
        }

        internal OxyColor GetColorFromMetric(ScorePointModel spoint)
        {
            if (spoint.Score == ScorePoints.Max(x => x.Score))
            {
                return OxyColors.Blue;
            }
            else if (spoint.Score == ScorePoints.Min(x => x.Score))
            {
                return OxyColors.Red;
            }
            if (ScorePoints.Any(x => x.bMetricChecked))
            {
                if (spoint.Score > ScorePoints.SingleOrDefault(x => x.bMetricChecked).Score)
                {
                    return OxyColors.Green;
                }
                else
                {
                    return OxyColors.Yellow;
                }
            }
            return OxyColors.Green;
        }

        private void OnDeleteScorePoint(Tuple<int, int> ids)
        {
            if (Id == ids.Item1)
            {
                ScorePoints.Remove(ScorePoints.FirstOrDefault(x => x.PointId == ids.Item2));
                foreach (var sp in ScorePoints.Where(x => x.PointId > ids.Item2))
                {
                    sp.PointId--;
                }
                OnAddPlotScorePoint(ids.Item1);
            }
        }

        private void OnPointUp(Tuple<int, int> ids)
        {
            if (Id == ids.Item1)
            {
                var temp_scorepoints = new List<ScorePointModel>();
                foreach (var sp in ScorePoints)
                {
                    if (sp.PointId == ids.Item2 - 1)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId + 1, EventAggregator));
                    }
                    else if (sp.PointId == ids.Item2)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId - 1, EventAggregator));
                    }
                    else
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId, EventAggregator));
                    }
                    temp_scorepoints.Last().PointX = sp.PointX;
                    temp_scorepoints.Last().Score = sp.Score;
                    if (sp.Colors.Colors.Count() == 3)
                    {
                        temp_scorepoints.Last().Colors = new Models.Internals.PlanScoreColorModel(new List<double>{
                        sp.Colors.Colors.First(),
                        sp.Colors.Colors.ElementAt(1),
                        sp.Colors.Colors.ElementAt(2) },
                            sp.Colors.ColorLabel);
                        //temp_scorepoints.Last().BackGroundBrush = new SolidColorBrush(Color.FromRgb((byte)sp.Colors.Colors.First(),
                        //    (byte)sp.Colors.Colors.ElementAt(1),
                        //    (byte)sp.Colors.Colors.ElementAt(2)));
                    }
                }
                ScorePoints.Clear();
                foreach (var t_sp in temp_scorepoints.OrderBy(x => x.PointId))
                {
                    ScorePoints.Add(new ScorePointModel(t_sp.MetricId, t_sp.PointId, EventAggregator)
                    {
                        PointX = t_sp.PointX,
                        Score = t_sp.Score,
                        Colors = t_sp.Colors,
                        //BackGroundBrush = t_sp.BackGroundBrush
                    });
                }
                OnAddPlotScorePoint(ids.Item1);
            }
        }

        private void OnPointDown(Tuple<int, int> ids)
        {
            if (Id == ids.Item1 && ids.Item2 < ScorePoints.Count())
            {
                var temp_scorepoints = new List<ScorePointModel>();
                foreach (var sp in ScorePoints)
                {
                    if (sp.PointId == ids.Item2 + 1)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId - 1, EventAggregator));
                    }
                    else if (sp.PointId == ids.Item2)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId + 1, EventAggregator));
                    }
                    else
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId, EventAggregator));
                    }
                    temp_scorepoints.Last().PointX = sp.PointX;
                    temp_scorepoints.Last().Score = sp.Score;
                    if (sp.Colors.Colors.Count() == 3)
                    {
                        temp_scorepoints.Last().Colors = new Models.Internals.PlanScoreColorModel(new List<double>{
                        sp.Colors.Colors.First(),
                        sp.Colors.Colors.ElementAt(1),
                        sp.Colors.Colors.ElementAt(2) },
                            sp.Colors.ColorLabel);
                        //temp_scorepoints.Last().BackGroundBrush = new SolidColorBrush(Color.FromRgb((byte)sp.Colors.Colors.First(),
                        //    (byte)sp.Colors.Colors.ElementAt(1),
                        //    (byte)sp.Colors.Colors.ElementAt(2)));
                    }
                }
                ScorePoints.Clear();
                foreach (var t_sp in temp_scorepoints.OrderBy(x => x.PointId))
                {
                    ScorePoints.Add(new ScorePointModel(t_sp.MetricId, t_sp.PointId, EventAggregator)
                    {
                        PointX = t_sp.PointX,
                        Score = t_sp.Score,
                        Colors = t_sp.Colors,
                        //BackGroundBrush = t_sp.BackGroundBrush
                    });
                }
                OnAddPlotScorePoint(ids.Item1);
            }
        }

        private void OnVariationChanged(Tuple<int, int> ids)
        {
            if (Id == ids.Item1 && ScorePoints.Count() != 0 && ScorePoints.Any(x => x.PointId == ids.Item2))
            {
                if (ScorePoints.FirstOrDefault(x => x.PointId == ids.Item2).bMetricChecked)
                {
                    //all other checkboxes except this one should be disabled.
                    foreach (var metric in ScorePoints)
                    {
                        if (metric.PointId != ids.Item2)
                        {
                            metric.bMidMetric = false;
                        }
                    }
                    OnAddPlotScorePoint(Id);
                }
                else
                {
                    OnAddPlotScorePoint(Id);
                }
            }
        }

        private void OnColorUpdate(Tuple<int, int, string, string> obj)
        {
            if (Id == obj.Item1 && ScorePoints.Count() != 0 && ScorePoints.Any(x => x.PointId == obj.Item2))
            {
                foreach (var metric in ScorePoints)
                {
                    if (metric.PointId == obj.Item2 && !obj.Item4.Contains("#00000000"))
                    {
                        try
                        {
                            byte byteA = Convert.ToByte(obj.Item4.Substring(3, 2), 16);
                            byte byteB = Convert.ToByte(obj.Item4.Substring(5, 2), 16);
                            byte byteC = Convert.ToByte(obj.Item4.Substring(7, 2), 16);

                            metric.Colors = new Models.Internals.PlanScoreColorModel(new List<double> { byteA, byteB, byteC }, $"{obj.Item3}[{metric.Score}]");
                        }
                        catch (Exception ex)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void SetEventAggregator(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }
    }
}
