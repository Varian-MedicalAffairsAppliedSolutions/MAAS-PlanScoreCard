using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels
{
    public class ScoreMetricViewModel : BindableBase
    {
        public DoseValueViewModel _doseValueViewModel { get; private set; }

        public HIViewModel _hiViewModel;
        private CIViewModel _ciViewModel;

        public VolumeAtDoseViewModel _volumeAtDoseViewModel { get; private set; }
        public DoseAtVolumeViewModel _doseAtVolumeViewModel { get; private set; }
        private int _metricId;
        public int MetricId
        {
            get { return _metricId; }
            set
            {
                SetProperty(ref _metricId, value);
                MetricUpCommand.RaiseCanExecuteChanged();
            }
        }

        private IEventAggregator _eventAggregator;

        //TODO Fill in scoremetric viewmodel (look at view).
        private ScoreMetricModel _scoreMetric;

        public string TemplateStructureId { get; }

        public ScoreMetricModel ScoreMetric
        {
            get { return _scoreMetric; }
            set { SetProperty(ref _scoreMetric, value); }
        }

        private bool _bExpanded;

        public bool BExpanded
        {
            get { return _bExpanded; }
            set { SetProperty(ref _bExpanded, value); }
        }
        private bool _bContracted;

        public bool BContracted
        {
            get { return _bContracted; }
            set { SetProperty(ref _bContracted, value); }
        }
        private StructureModel _structureTxt;

        public StructureModel StructureTxt
        {
            get { return _structureTxt; }
            set
            {
                SetProperty(ref _structureTxt, value);
                if (StructureTxt != null)
                {
                    ScoreMetric.Structure = StructureTxt;
                }
            }
        }

        private string _metricTxt;

        public string MetricTxt
        {
            get { return _metricTxt; }
            set { SetProperty(ref _metricTxt, value); }
        }

        public string SelectedDoseValueMetric { get; set; }
        public ObservableCollection<StructureModel> StructureIds { get; set; }
        public DelegateCommand AddPointCommand { get; private set; }
        public PlotModel ScoreMetricPlotModel { get; set; }
        public DelegateCommand ExpandMetricDetailsCommand { get; private set; }
        public DelegateCommand ContractMetricDetailsCommand { get; private set; }
        public DelegateCommand DeleteMetricCommand { get; private set; }
        public DelegateCommand CopyMetricCommand { get; private set; }
        public DelegateCommand MetricUpCommand { get; private set; }
        public DelegateCommand MetricDownCommand { get; private set; }
        public DelegateCommand ReOrderMetricsCommand { get; private set; }
        public ScoreMetricViewModel(DoseAtVolumeViewModel doseAtVolumeViewModel,
            VolumeAtDoseViewModel volumeAtDoseViewModel,
            DoseValueViewModel doseValueViewModel,
            HIViewModel hiViewModel,
            CIViewModel ciViewModel,
            MetricTypeEnum metricType,
            int metricId, //not sure this is necessary. 
            StructureModel selectedStructure,
            IEnumerable<StructureModel> structureIds,
            IEventAggregator eventAggregator)
        {
            _metricId = metricId;
            _doseAtVolumeViewModel = doseAtVolumeViewModel;
            _volumeAtDoseViewModel = volumeAtDoseViewModel;
            _doseValueViewModel = doseValueViewModel;
            _hiViewModel = hiViewModel;
            _ciViewModel = ciViewModel;
            _eventAggregator = eventAggregator;
            StructureIds = new ObservableCollection<StructureModel>();
            foreach (var s in structureIds)
            {
                StructureIds.Add(s);
            }
            TemplateStructureId = selectedStructure.StructureId;
            ScoreMetric = GetScoreMetricFromMetricType(metricType, selectedStructure);
            if (ScoreMetric != null)
            {
                SetEvents();
                ScoreMetricPlotModel = new PlotModel();
                SetPlotProperties(metricType);
                StructureTxt = StructureIds.FirstOrDefault(x => x.StructureId == selectedStructure.StructureId);
                MetricTxt = GetMetricText(metricType);//                
            }
            SetCommands();//commands still need to work so you can delete the metrics that don't exist. 
        }
        /// <summary>
        /// Get Metric text From Metric Type to display on the UI
        /// </summary>
        /// <param name="metricType">Enum for metric type</param>
        /// <returns></returns>
        internal string GetMetricText(MetricTypeEnum metricType)
        {
            switch (metricType)
            {
                case MetricTypeEnum.DoseAtVolume:
                    return $"Dose at {_doseAtVolumeViewModel.Volume}{_doseAtVolumeViewModel.SelectedVolumeUnit}";
                case MetricTypeEnum.VolumeAtDose:
                    return $"Volume at {_volumeAtDoseViewModel.Dose}{_volumeAtDoseViewModel.SelectedDoseUnit}";
                case MetricTypeEnum.MinDose:
                    return $"Min Dose [{_doseValueViewModel.SelectedDoseUnit}]";
                case MetricTypeEnum.MeanDose:
                    return $"Mean Dose [{_doseValueViewModel.SelectedDoseUnit}]";
                case MetricTypeEnum.MaxDose:
                    return $"Max Dose [{_doseValueViewModel.SelectedDoseUnit}]";
                case MetricTypeEnum.VolumeOfRegret:
                    return $"Vol of regret at {_volumeAtDoseViewModel.Dose}{_volumeAtDoseViewModel.SelectedDoseUnit}";
                case MetricTypeEnum.ConformationNumber:
                    return $"Conf No. at {_volumeAtDoseViewModel.Dose}{_volumeAtDoseViewModel.SelectedDoseUnit}";
                case MetricTypeEnum.HomogeneityIndex:
                    return $"HI [D{_hiViewModel.HI_HiValue}-D{_hiViewModel.HI_LowValue}]/{_hiViewModel.TargetValue}";
                case MetricTypeEnum.ConformityIndex:
                    return $"CI [{_ciViewModel.Dose} [{_ciViewModel.SelectedDoseUnit}]]";
                default:
                    return "Undefined Metric";
            }
        }
        /// <summary>
        /// Convert the metric type and structure from the template to a ScoreMetricModel
        /// </summary>
        /// <param name="metricType">metric type enum</param>
        /// <param name="selectedStructure">StructureModel template structure.</param>
        /// <returns></returns>
        internal ScoreMetricModel GetScoreMetricFromMetricType(MetricTypeEnum metricType, StructureModel selectedStructure)
        {
            if (metricType == MetricTypeEnum.DoseAtVolume)
            {
                return new ScoreMetricModel
                {
                    Id = _metricId,
                    Structure = selectedStructure,
                    MetricType = metricType,
                    InputValue = _doseAtVolumeViewModel.Volume,
                    InputUnit = _doseAtVolumeViewModel.SelectedVolumeUnit,
                    OutputUnit = _doseAtVolumeViewModel.SelectedDoseUnit
                };
            }
            else if (metricType == MetricTypeEnum.VolumeAtDose || metricType == MetricTypeEnum.VolumeOfRegret || metricType == MetricTypeEnum.ConformationNumber)
            {
                return new ScoreMetricModel
                {
                    Id = _metricId,
                    Structure = selectedStructure,
                    MetricType = metricType,
                    InputValue = _volumeAtDoseViewModel.Dose,
                    OutputUnit = _volumeAtDoseViewModel.SelectedVolumeUnit,
                    InputUnit = _volumeAtDoseViewModel.SelectedDoseUnit
                };
            }
            else if (metricType == MetricTypeEnum.MaxDose || metricType == MetricTypeEnum.MinDose || metricType == MetricTypeEnum.MeanDose)
            {
                return new ScoreMetricModel
                {
                    Id = _metricId,
                    Structure = selectedStructure,
                    MetricType = metricType,//GetEnumFromMetricString(SelectedDoseValueMetric),
                    OutputUnit = _doseValueViewModel.SelectedDoseUnit,
                };
            }
            else if (metricType == MetricTypeEnum.HomogeneityIndex)
            {
                return new ScoreMetricModel
                {
                    Id = _metricId,
                    Structure = selectedStructure,
                    MetricType = metricType,
                    HI_Hi = _hiViewModel.HI_HiValue.ToString(),
                    HI_Lo = _hiViewModel.HI_LowValue.ToString(),
                    HI_Target = _hiViewModel.TargetValue.ToString(),
                    InputUnit = _hiViewModel.SelectedDoseUnit
                };
            }
            else if (metricType == MetricTypeEnum.ConformityIndex)
            {
                return new ScoreMetricModel
                {
                    Id = _metricId,
                    Structure = selectedStructure,
                    MetricType = metricType,
                    InputValue = _ciViewModel.Dose.ToString(),
                    InputUnit = _ciViewModel.SelectedDoseUnit,
                    OutputUnit = "CC"
                };
            }
            return null;
        }

        private void SetEvents()
        {
            //StructureIds = new ObservableCollection<string>();
            _eventAggregator.GetEvent<AddScorePointEvent>().Subscribe(OnAddPlotScorePoint);
            _eventAggregator.GetEvent<DeleteScorePointEvent>().Subscribe(OnDeleteScorePoint);
            _eventAggregator.GetEvent<PointUpEvent>().Subscribe(OnPointUp);
            _eventAggregator.GetEvent<PointDownEvent>().Subscribe(OnPointDown);
            _eventAggregator.GetEvent<VariationCheckedEvent>().Subscribe(OnVariationChanged);
            _eventAggregator.GetEvent<ColorUpdateEvent>().Subscribe(OnColorUpdate);

        }

        private void SetCommands()
        {
            AddPointCommand = new DelegateCommand(OnAddMetricPoint);
            ExpandMetricDetailsCommand = new DelegateCommand(OnExpandMetricDetails);
            ContractMetricDetailsCommand = new DelegateCommand(OnContractMetricDetails);
            DeleteMetricCommand = new DelegateCommand(OnDeleteMetric);
            CopyMetricCommand = new DelegateCommand(OnCopyMetric);
            MetricUpCommand = new DelegateCommand(OnMetricUp, CanMetricUp);
            MetricDownCommand = new DelegateCommand(OnMetricDown);
            ReOrderMetricsCommand = new DelegateCommand(OnReorderMetrics);
            BExpanded = true;
        }

        /// <summary>
        /// Convert selected Metric to an enum
        /// </summary>
        /// <param name="selectedDoseValueMetric">Selected Dose Value (min, max, mean)</param>
        /// <returns></returns>
        internal MetricTypeEnum GetEnumFromMetricString(string selectedDoseValueMetric)
        {
            if (selectedDoseValueMetric.Contains("Min"))
            {
                return MetricTypeEnum.MinDose;
            }
            else if (selectedDoseValueMetric.Contains("Max"))
            {
                return MetricTypeEnum.MaxDose;
            }
            else if (selectedDoseValueMetric.Contains("Mean"))
            {
                return MetricTypeEnum.MeanDose;
            }
            else
            {
                return MetricTypeEnum.Undefined;
            }
        }

        private void OnDeleteMetric()
        {
            _eventAggregator.GetEvent<DeleteMetricEvent>().Publish(_metricId);
        }
        private void OnCopyMetric()
        {
            if (this.ScoreMetric != null)
            {
                _eventAggregator.GetEvent<CopyMetricEvent>().Publish(this);
            }
        }
        private bool CanMetricUp()
        {
            return this.MetricId > 0;
        }
        private void OnMetricUp()
        {
            _eventAggregator.GetEvent<MetricUpEvent>().Publish(MetricId);
        }

        private void OnMetricDown()
        {
            _eventAggregator.GetEvent<MetricDownEvent>().Publish(MetricId);
        }
        private void OnContractMetricDetails()
        {
            BContracted = true;
            BExpanded = false;
        }

        private void OnExpandMetricDetails()
        {
            BExpanded = true;
            BContracted = false;
        }

        public void OnAddPlotScorePoint(int obj)
        {
            SetVariation();//set the visibilty status again for when the score changes.
            if (this._metricId == obj)
            {
                ScoreMetricPlotModel.Series.Clear();
                //break scorepoints into 2 groups, before and after the variation.
                //this one sets marker color.
                foreach (var spoint in ScoreMetric.ScorePoints.OrderBy(x => x.PointX))
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
                    ScorePointSeries.Points.Add(new DataPoint(spoint.PointX, spoint.Score));
                    ScoreMetricPlotModel.Series.Add(ScorePointSeries);
                }
                if (ScoreMetric.ScorePoints.Any(x => x.bMetricChecked))
                {
                    var PointSeriesAbove = new LineSeries
                    {
                        Color = OxyColors.Green
                    };
                    foreach (var spoint in ScoreMetric.ScorePoints.Where(x => x.Score >= ScoreMetric.ScorePoints.SingleOrDefault(y => y.bMetricChecked).Score).OrderBy(x => x.PointX))
                    {

                        //add to the plot
                        PointSeriesAbove.Points.Add(new DataPoint(spoint.PointX, spoint.Score));
                    }
                    ScoreMetricPlotModel.Series.Add(PointSeriesAbove);

                    var PointSeriesBelow = new LineSeries
                    {
                        Color = OxyColors.Yellow
                    };
                    foreach (var spoint in ScoreMetric.ScorePoints.Where(x => x.Score <= ScoreMetric.ScorePoints.SingleOrDefault(y => y.bMetricChecked).Score).OrderBy(x => x.PointX))
                    {
                        //add to the plot
                        PointSeriesBelow.Points.Add(new DataPoint(spoint.PointX, spoint.Score));
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
                    foreach (var spoint in ScoreMetric.ScorePoints.OrderBy(x => x.PointX))
                    {
                        //add to the plot
                        PointSeriesAllGreen.Points.Add(new DataPoint(spoint.PointX, spoint.Score));
                    }
                    ScoreMetricPlotModel.Series.Add(PointSeriesAllGreen);
                }
                ScoreMetricPlotModel.InvalidatePlot(true);
            }
        }
        /// <summary>
        /// Determines oxyplot color based on where along the line the current point is located
        /// </summary>
        /// <param name="spoint">point along score objective.</param>
        /// <returns></returns>
        internal OxyColor GetColorFromMetric(ScorePointModel spoint)
        {
            if (spoint.Score == ScoreMetric.ScorePoints.Max(x => x.Score))
            {
                return OxyColors.Blue;
            }
            else if (spoint.Score == ScoreMetric.ScorePoints.Min(x => x.Score))
            {
                return OxyColors.Red;
            }
            if (ScoreMetric.ScorePoints.Any(x => x.bMetricChecked))
            {
                if (spoint.Score > ScoreMetric.ScorePoints.SingleOrDefault(x => x.bMetricChecked).Score)
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

        private void SetPlotProperties(MetricTypeEnum metricType)
        {
            ScoreMetricPlotModel.Title = PlanScorePlottingServices.GetPlotTitle(metricType,_doseAtVolumeViewModel,_volumeAtDoseViewModel);// $"Score for Dose at {doseAtVolumeViewModel.Volume}{doseAtVolumeViewModel.SelectedVolumeUnit}";
            ScoreMetricPlotModel.LegendPlacement = LegendPlacement.Outside;
            ScoreMetricPlotModel.LegendPosition = LegendPosition.RightTop;
            ScoreMetricPlotModel.Axes.Add(new LinearAxis
            {
                Title = PlanScorePlottingServices.GetPlotXAxisTitle(metricType,_doseAtVolumeViewModel,_volumeAtDoseViewModel,_doseValueViewModel),// $"Dose [{doseAtVolumeViewModel.SelectedDoseUnit}]",
                Position = AxisPosition.Bottom,
            });
            ScoreMetricPlotModel.Axes.Add(new LinearAxis
            {
                Title = $"Score",
                Position = AxisPosition.Left
            });
        }
      

        private void OnAddMetricPoint()
        {
            ScoreMetric.ScorePoints.Add(new ScorePointModel(_metricId, ScoreMetric.ScorePoints.Count(), _eventAggregator));
            //determine whether the validation checkbox is enabled.
            SetVariation();
        }

        //this should be an event it gets used by other classes.
        public void SetVariation()
        {
            //if (!ScoreMetric.ScorePoints.Any(x => x.bMidMetric))
            //{
            if (ScoreMetric.ScorePoints.Any(x => x.bMetricChecked))
            {
                ScoreMetric.ScorePoints.FirstOrDefault(x => x.bMetricChecked).bMidMetric = true;
                return;//don't set the enableness if a metric is already checked.
            }
            foreach (var metric in ScoreMetric.ScorePoints)
            {
                if (!(metric.Score == ScoreMetric.ScorePoints.Max(x => x.Score) || metric.Score == ScoreMetric.ScorePoints.Min(x => x.Score)))
                {
                    metric.bMidMetric = true;
                }
                else
                {
                    metric.bMidMetric = false;
                }
            }
        }

        private void OnVariationChanged(Tuple<int, int> ids)
        {
            if (MetricId == ids.Item1 && ScoreMetric.ScorePoints.Count() != 0 && ScoreMetric.ScorePoints.Any(x => x.PointId == ids.Item2))
            {
                if (ScoreMetric.ScorePoints.FirstOrDefault(x => x.PointId == ids.Item2).bMetricChecked)
                {
                    //all other checkboxes except this one should be disabled.
                    foreach (var metric in ScoreMetric.ScorePoints)
                    {
                        if (metric.PointId != ids.Item2)
                        {
                            metric.bMidMetric = false;
                        }
                    }
                    OnAddPlotScorePoint(MetricId);
                }
                else
                {
                    OnAddPlotScorePoint(MetricId);
                }
            }
        }
        private void OnColorUpdate(Tuple<int, int, string, string> obj)
        {
            if (MetricId == obj.Item1 && ScoreMetric.ScorePoints.Count() != 0 && ScoreMetric.ScorePoints.Any(x => x.PointId == obj.Item2))
            {
                foreach (var metric in ScoreMetric.ScorePoints)
                {
                    if (metric.PointId == obj.Item2 && !obj.Item4.Contains("#00000000"))
                    {
                        metric.Colors = new Models.Internals.PlanScoreColorModel(new List<double>
                        {
                            Convert.ToByte(obj.Item4.Substring(3,2),16),
                            Convert.ToByte(obj.Item4.Substring(5,2),16),
                            Convert.ToByte(obj.Item4.Substring(7,2),16)
                        }, $"{obj.Item3}[{metric.Score}]");
                    }
                }
            }
        }
        private void OnDeleteScorePoint(Tuple<int, int> ids)
        {
            if (this._metricId == ids.Item1)
            {
                ScoreMetric.ScorePoints.Remove(ScoreMetric.ScorePoints.FirstOrDefault(x => x.PointId == ids.Item2));
                foreach (var sp in ScoreMetric.ScorePoints.Where(x => x.PointId > ids.Item2))
                {
                    sp.PointId--;
                }
                OnAddPlotScorePoint(ids.Item1);
            }
        }
        private void OnPointDown(Tuple<int, int> ids)
        {
            if (this._metricId == ids.Item1 && ids.Item2 < ScoreMetric.ScorePoints.Count())
            {
                var temp_scorepoints = new List<ScorePointModel>();
                foreach (var sp in ScoreMetric.ScorePoints)
                {
                    if (sp.PointId == ids.Item2 + 1)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId - 1, _eventAggregator));
                    }
                    else if (sp.PointId == ids.Item2)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId + 1, _eventAggregator));
                    }
                    else
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId, _eventAggregator));
                    }
                    temp_scorepoints.Last().PointX = sp.PointX;
                    temp_scorepoints.Last().Score = sp.Score;
                }
                ScoreMetric.ScorePoints.Clear();
                foreach (var t_sp in temp_scorepoints.OrderBy(x => x.PointId))
                {
                    ScoreMetric.ScorePoints.Add(new ScorePointModel(t_sp.MetricId, t_sp.PointId, _eventAggregator)
                    {
                        PointX = t_sp.PointX,
                        Score = t_sp.Score
                    });
                }
                OnAddPlotScorePoint(ids.Item1);
            }
        }

        private void OnPointUp(Tuple<int, int> ids)
        {
            if (this._metricId == ids.Item1)
            {
                var temp_scorepoints = new List<ScorePointModel>();
                foreach (var sp in ScoreMetric.ScorePoints)
                {
                    if (sp.PointId == ids.Item2 - 1)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId + 1, _eventAggregator));
                    }
                    else if (sp.PointId == ids.Item2)
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId - 1, _eventAggregator));
                    }
                    else
                    {
                        temp_scorepoints.Add(new ScorePointModel(sp.MetricId, sp.PointId, _eventAggregator));
                    }
                    temp_scorepoints.Last().PointX = sp.PointX;
                    temp_scorepoints.Last().Score = sp.Score;
                }
                ScoreMetric.ScorePoints.Clear();
                foreach (var t_sp in temp_scorepoints.OrderBy(x => x.PointId))
                {
                    ScoreMetric.ScorePoints.Add(new ScorePointModel(t_sp.MetricId, t_sp.PointId, _eventAggregator)
                    {
                        PointX = t_sp.PointX,
                        Score = t_sp.Score
                    });
                }
                OnAddPlotScorePoint(ids.Item1);
            }
        }

        private void OnReorderMetrics()
        {
            var tempPoints = new ScoreMetricModel();
            foreach (var point in ScoreMetric.ScorePoints)
            {
                tempPoints.ScorePoints.Add(point);
            }
            ScoreMetric.ScorePoints.Clear();
            int point_count = 0;
            foreach (var tpoint in tempPoints.ScorePoints.OrderBy(x => x.PointX))
            {
                ScoreMetric.ScorePoints.Add(new ScorePointModel(MetricId, point_count, _eventAggregator));
                ScoreMetric.ScorePoints.Last().PointX = tpoint.PointX;
                ScoreMetric.ScorePoints.Last().Score = tpoint.Score;
                ScoreMetric.ScorePoints.Last().bMidMetric = tpoint.bMidMetric;
                ScoreMetric.ScorePoints.Last().bMetricChecked = tpoint.bMetricChecked;
                point_count++;
            }
        }
    }
}
