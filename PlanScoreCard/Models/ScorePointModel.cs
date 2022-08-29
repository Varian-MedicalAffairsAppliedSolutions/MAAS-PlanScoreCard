﻿using PlanScoreCard.Events;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.ViewModels;
using PlanScoreCard.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace PlanScoreCard.Models
{
    public class ScorePointModel : BindableBase , INotifyPropertyChanged
    {

        public bool CanReOrder { get; set; }

        private System.Windows.Media.Brush planScoreBackgroundColor;

        public System.Windows.Media.Brush PlanScoreBackgroundColor
        {
            get { return planScoreBackgroundColor; }
            set 
            { 
                SetProperty(ref planScoreBackgroundColor, value);
                NotifyPropertyChanged();
            }
        }

        private double _pointX;

        public double PointX
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
                NotifyPropertyChanged();
                _eventAggregator.GetEvent<VariationCheckedEvent>().Publish(new Tuple<int, int>(MetricId, PointId));
            }
        }

        private PlanScoreColorModel _colors;

        public PlanScoreColorModel Colors
        {
            get { return _colors; }
            set 
            { 
                SetProperty(ref _colors, value);
                if (_colors != null)
                {
                    _colors.Colors = value.Colors;
                    _colors.ColorValue = value.ColorValue;
                    _colors.ColorLabel = value.ColorLabel;
                }
            }
        }

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
                
                PlanScoreBackgroundColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(obj.Item4));
                //if (obj.Item4.Contains("#00000000"))
                //{
                //    Colors = new PlanScoreColorModel(new List<double> { Convert.ToByte(obj.Item4.Substring(3,2),16),
                //    Convert.ToByte(obj.Item4.Substring(5,2),16),
                //    Convert.ToByte(obj.Item4.Substring(7,2),16)},
                //    $"{obj.Item3}[{Score}]");
                //}
                //else
                //{
                    byte byteA = Convert.ToByte(obj.Item4.Substring(3, 2), 16);
                    byte byteB = Convert.ToByte(obj.Item4.Substring(5, 2), 16);
                    byte byteC = Convert.ToByte(obj.Item4.Substring(7, 2), 16);

                    Colors = new Models.Internals.PlanScoreColorModel(new List<double> { byteA, byteB, byteC }, $"{obj.Item3}[{Score}]");
                
                //}
                //if its a deleted metric the application is still holding onto the metricid and point id (check null colorpicker). 
                //set other empty colors to white so that they don't get skiped.

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

        public event PropertyChangedEventHandler PropertyChanged;
        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
