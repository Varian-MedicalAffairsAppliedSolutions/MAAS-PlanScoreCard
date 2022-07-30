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
using System.Runtime.CompilerServices;

namespace PlanScoreCard.Models
{
    public class ScoreMetricModel : BindableBase, INotifyPropertyChanged
    {
        public bool IsLoaded { get; set; }

        public IEventAggregator EventAggregator;

        // Metric Type

        private MetricTypeEnum metricType;

        public MetricTypeEnum MetricType
        {
            get { return metricType; }
            set 
            { 
                SetProperty(ref metricType , value);
                EventAggregator.GetEvent<UpdateMetricEditorEvent>().Publish();
                UpdateMetricText();
                ScoreMetricPlotModel.Title = PlanScorePlottingServices.GetPlotTitle(metricType, this);
                NotifyPropertyChanged();
            }
        }

        // MetricName
        private string metricText;

        public string MetricText
        {
            get { return metricText; }
            set 
            { 
                SetProperty(ref metricText, value);
                NotifyPropertyChanged();
            }
        }

        private void UpdateMetricText()
        {
            switch (MetricType)
            {
                case MetricTypeEnum.DoseAtVolume:
                    MetricText = $"Dose at {InputValue}{InputUnit}";
                    return;
                case MetricTypeEnum.VolumeAtDose:
                    MetricText = $"Volume at {InputValue}{InputUnit}";
                    return;
                case MetricTypeEnum.MinDose:
                    MetricText = $"Min Dose [{OutputUnit}]";
                    return;
                case MetricTypeEnum.MeanDose:
                    MetricText = $"Mean Dose [{OutputUnit}]";
                    return;
                case MetricTypeEnum.MaxDose:
                    MetricText = $"Max Dose [{OutputUnit}]";
                    return;
                case MetricTypeEnum.VolumeOfRegret:
                    MetricText = $"Vol of regret at {InputValue}{InputUnit}";
                    return;
                case MetricTypeEnum.ConformationNumber:
                    MetricText = $"Conf No. at {InputValue}{InputUnit}";
                    return;
                case MetricTypeEnum.HomogeneityIndex:
                    MetricText = $"HI [D{HI_Hi}-D{HI_Lo}]/{HI_Target}";
                    return;
                case MetricTypeEnum.ConformityIndex:
                    MetricText = $"CI [{InputValue} [{InputUnit}]]";
                    return;
                case MetricTypeEnum.InhomogeneityIndex:
                    MetricText = $"Inhomogeneity Index";
                    return;
                case MetricTypeEnum.ModifiedGradientIndex:
                    MetricText = $"Modified GI [{HI_Lo}/{HI_Hi}]";
                    return;
                case MetricTypeEnum.DoseAtSubVolume:
                    MetricText = $"D At (V - {InputValue}cc)";
                    return;
                default:
                    MetricText = "Undefined Metric";
                    return;
            }
        }

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
                NotifyPropertyChanged();
            }
        }

        // Metric Structure
        private StructureModel structure;

        public StructureModel Structure
        {
            get { return structure; }
            set
            {
                if (value == null)
                    return;
                
                StructureModel structureModel = Structures.FirstOrDefault(s => s.StructureId == value.StructureId);

                if (structureModel == null) // Try matching based off of Structure Code
                {
                    if(value.StructureCode != null)
                        structureModel = Structures.FirstOrDefault(s => s.StructureCode == value.StructureCode);

                    if (structureModel == null && !String.IsNullOrWhiteSpace(value.TemplateStructureId))
                    {
                        structureModel = new StructureModel { TemplateStructureId = value.TemplateStructureId };
                    }
                    else
                    {
                        structureModel = new StructureModel { TemplateStructureId = value.StructureId };
                    }
                }
                structureModel.AutoGenerated = value.AutoGenerated;
                structureModel.StructureCode = value.StructureCode;
                structureModel.StructureComment = value.StructureComment;
                structureModel.TemplateStructureId = value.TemplateStructureId;
                value = structureModel;
                EventAggregator.GetEvent<MetricStructureChangedEvent>().Publish();
                SetProperty(ref structure, value);
                
                //if(IsLoaded)
                //    Structure.TemplateStructureId = value.StructureId;
                
                NotifyPropertyChanged();
            }
        }
        private string _metricComment;

        public string MetricComment
        {
            get { return _metricComment; }
            set 
            {
                SetProperty(ref _metricComment,value);
            }
        }

        // Available Structures
        public ObservableCollection<StructureModel> Structures { get; set; }

        // ScorePoints
        private ObservableCollection<ScorePointModel> scorePoints;

        public ObservableCollection<ScorePointModel> ScorePoints
        {
            get { return scorePoints; }
            set { SetProperty(ref scorePoints , value); }
        }

        // ScorePoint Plot

        private ViewResolvingPlotModel scoreMetricPlotModel;

        public ViewResolvingPlotModel ScoreMetricPlotModel
        {
            get { return scoreMetricPlotModel; }
            set 
            { 
                SetProperty(ref scoreMetricPlotModel , value);
                NotifyPropertyChanged();
            }
        }

        // Metric Parameters
        private string inputValue;

        public string InputValue
        {
            get { return inputValue; }
            set 
            { 
                SetProperty( ref inputValue , value);
                UpdateMetricText();
                ScoreMetricPlotModel.Title = PlanScorePlottingServices.GetPlotTitle(metricType, this);
                NotifyPropertyChanged();
            }
        }

        private string inputUnit;

        public string InputUnit
        {
            get { return inputUnit; }
            set 
            { 
                SetProperty( ref inputUnit , value);
                UpdateMetricText();
                ScoreMetricPlotModel.Title = PlanScorePlottingServices.GetPlotTitle(metricType, this);
                NotifyPropertyChanged();
            }
        }

        private string outputUnit;

        public string OutputUnit
        {
            get { return outputUnit; }
            set 
            { 
                SetProperty(ref outputUnit , value);
                UpdateMetricText();
                ScoreMetricPlotModel.Title = PlanScorePlottingServices.GetPlotTitle(metricType, this);
                NotifyPropertyChanged();
            }
        }

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
            IsLoaded = false;

            SetEvents();

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

        public void SetPlotProperties(MetricTypeEnum metricType)
        {
            ScoreMetricPlotModel.Axes.Clear();


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

        public void OnAddPlotScorePoint(int obj)
        {
            SetVariation();//set the visibilty status again for when the score changes.
            if (Id == obj)
            {
                ScoreMetricPlotModel.Series.Clear();
                //break scorepoints into 2 groups, before and after the variation.
                //this one sets marker color.
                foreach (ScorePointModel spoint in ScorePoints.OrderBy(x => x.PointX))
                {
                    var ScorePointSeries = new LineSeries { Color = GetColorFromMetric(spoint), MarkerType = MarkerType.Diamond, MarkerSize = 8};
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
                //EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
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
        //NOT USED
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
        //NOT USED
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
            if (Id == ids.Item1+1 && ScorePoints.Count() != 0 && ScorePoints.Any(x => x.PointId == ids.Item2))
            {
                if (ScorePoints.FirstOrDefault(x => x.PointId == ids.Item2).bMetricChecked)
                {
                    //all other checkboxes except this one should be disabled.
                    foreach (var metric in ScorePoints)
                    {
                        if (metric.PointId != ids.Item2)
                        {
                            metric.bMetricChecked = metric.bMidMetric =  false;
                        }
                    }
                    OnAddPlotScorePoint(Id);
                }
                if(ScorePoints.All(x=>!x.bMetricChecked))
                {
                    OnAddPlotScorePoint(Id);
                }
            }
        }

        private void OnColorUpdate(Tuple<int, int, string, string> obj)
        {
            //if (Id == obj.Item1 && ScorePoints.Count() != 0 && ScorePoints.Any(x => x.PointId == obj.Item2))
            if(ScorePoints.Any(x=>x.MetricId == obj.Item1 && x.PointId == obj.Item2))
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

                            System.Windows.Media.SolidColorBrush PlanScoreBackgroundColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(byteA,byteB,byteC));

                            EventAggregator.GetEvent<UpdateScorePointGridEvent>().Publish(PlanScoreBackgroundColor);
                        }
                        catch (Exception ex)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public void SetEventAggregator(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }
    }
}
