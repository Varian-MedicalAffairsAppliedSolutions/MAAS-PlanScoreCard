using PlanScoreCard.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Windows.Media;

namespace PlanScoreCard.ViewModels
{
    public class ColorPickerViewModel : BindableBase
    {
        private int _metricId;
        private int _pointId;
        private string _colorLabel;

        public string ColorLabel
        {
            get { return _colorLabel; }
            set { SetProperty(ref _colorLabel, value); }
        }

        private IEventAggregator _eventAggregator;

        public DelegateCommand<Brush> SelectColorCommand { get; private set; }
        public DelegateCommand CancelColorCommand { get; private set; }
        public ColorPickerViewModel(int metricId, int pointId, string label, IEventAggregator eventAggregator)
        {
            _metricId = metricId;
            _pointId = pointId;
            ColorLabel = label;
            _eventAggregator = eventAggregator;
            SelectColorCommand = new DelegateCommand<Brush>(OnSelectColor);
            CancelColorCommand = new DelegateCommand(OnCancelColor);
        }

        private void OnCancelColor()
        {
            //System.Windows.MessageBox.Show($"Cancelled!");
            _eventAggregator.GetEvent<ColorSelectedEvent>().Publish(new Tuple<int, int, string, string>(_metricId, _pointId, ColorLabel, "#00000000"));
        }

        private void OnSelectColor(Brush obj)
        {
            //System.Windows.MessageBox.Show($"Color Selected {obj.ToString()}");
            _eventAggregator.GetEvent<ColorSelectedEvent>().Publish(new Tuple<int, int, string, string>(_metricId, _pointId, ColorLabel, obj.ToString()));
        }
    }
}
