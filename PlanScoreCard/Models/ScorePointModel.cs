using PlanScoreCard.Events;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.ViewModels;
using PlanScoreCard.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace PlanScoreCard.Models
{
    public class ScorePointModel : BindableBase
    {

        public bool CanReOrder { get; set; }

        private System.Windows.Media.Brush planScoreBackgroundColor;

        public System.Windows.Media.Brush PlanScoreBackgroundColor
        {
            get { return planScoreBackgroundColor; }
            set { SetProperty(ref planScoreBackgroundColor, value); }
        }

        private decimal _pointX;

        public decimal PointX
        {
            get { return _pointX; }
            set
            {
                SetProperty(ref _pointX, value);
                _eventAggregator.GetEvent<AddScorePointEvent>().Publish(MetricId);
                _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }

        private int _metricId;
        public int MetricId
        {
            get { return _metricId; }
            set { SetProperty(ref _metricId, value); }
        }
        private int _pointId;
        public int PointId
        {
            get { return _pointId; }
            set 
            { 
                SetProperty(ref _pointId, value);
                if(_eventAggregator != null && CanReOrder)
                    _eventAggregator.GetEvent<ReRankMetricPointsEvent>().Publish();
            }
        }
        private double _score;

        private IEventAggregator _eventAggregator;

        public double Score
        {
            get { return _score; }
            set
            {
                SetProperty(ref _score, value);
                _eventAggregator.GetEvent<AddScorePointEvent>().Publish(MetricId);
                _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }
        private bool _bMidMetric;

        public bool bMidMetric
        {
            get { return _bMidMetric; }
            set { SetProperty(ref _bMidMetric, value); }
        }
        private bool _bMetricChecked;
        private ColorPickerView _colorPicker;

        public bool bMetricChecked
        {
            get { return _bMetricChecked; }
            set
            {
                SetProperty(ref _bMetricChecked, value);
                _eventAggregator.GetEvent<VariationCheckedEvent>().Publish(new Tuple<int, int>(MetricId, PointId));
            }
        }

        //private System.Windows.Media.Brush _backGroundBrush;

        //public System.Windows.Media.Brush BackGroundBrush
        //{
        //    get { return _backGroundBrush; }
        //    set { SetProperty(ref _backGroundBrush, value); }
        //}
        private PlanScoreColorModel _colors;

        public PlanScoreColorModel Colors
        {
            get { return _colors; }
            set 
            { 
                SetProperty(ref _colors, value);
                _colors.Colors = value.Colors;
                _colors.ColorValue = value.ColorValue;
                _colors.ColorLabel = value.ColorLabel;
            }
        }

        //public PlanScoreColorModel Colors { get; set; }

        public DelegateCommand DeletePointCommand { get; private set; }
        public DelegateCommand PointUpCommand { get; private set; }
        public DelegateCommand PointDownCommand { get; private set; }
        public DelegateCommand LaunchColorPickerCommand { get; private set; }
        public ScorePointModel(int metricId, int pointId, IEventAggregator eventAggregator)
        {
            MetricId = metricId;
            PointId = pointId;
            CanReOrder = true;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ColorSelectedEvent>().Subscribe(OnColorSelected);
            DeletePointCommand = new DelegateCommand(OnDeletePoint);
            PointUpCommand = new DelegateCommand(OnPointUp, CanPointUp);
            PointDownCommand = new DelegateCommand(OnPointDown);
            LaunchColorPickerCommand = new DelegateCommand(OnLaunchColorPicker);
        }

        /// <summary>
        /// Converts the input object to a PlanScoreColorModel
        /// </summary>
        /// <param name="obj">Tuple contains Metric Id, Point Id, Color and Label</param>
        internal void OnColorSelected(Tuple<int, int, string, string> obj)
        {
            if (MetricId == obj.Item1 && PointId == obj.Item2)
            {
                //BackGroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(obj.Item4));
                if (obj.Item4.Contains("#00000000"))
                {
                    Colors = new PlanScoreColorModel(new List<double> { Convert.ToByte(obj.Item4.Substring(3,2),16),
                    Convert.ToByte(obj.Item4.Substring(5,2),16),
                    Convert.ToByte(obj.Item4.Substring(7,2),16)},
                    $"{obj.Item3}[{Score}]");
                }
                //if its a deleted metric the application is still holding onto the metricid and point id (check null colorpicker). 
                if (_colorPicker != null) { _colorPicker.Close(); }
                _eventAggregator.GetEvent<ColorUpdateEvent>().Publish(obj);
            }
        }

        private void OnLaunchColorPicker()
        {
            _colorPicker = new ColorPickerView();
            _colorPicker.DataContext = new ColorPickerViewModel(MetricId, PointId, Colors != null ? Colors.ColorLabel.Split('[').First() : String.Empty, _eventAggregator);
            _colorPicker.ShowDialog();
        }

        private bool CanPointUp()
        {
            return PointId != 0;
        }

        private void OnPointDown()
        {
            _eventAggregator.GetEvent<PointDownEvent>().Publish(new Tuple<int, int>(MetricId, PointId));
        }

        private void OnPointUp()
        {
            _eventAggregator.GetEvent<PointUpEvent>().Publish(new Tuple<int, int>(MetricId, PointId));
        }

        private void OnDeletePoint()
        {
            _eventAggregator.GetEvent<DeleteScorePointEvent>().Publish(new Tuple<int, int>(MetricId, PointId));
        }
    }
}
