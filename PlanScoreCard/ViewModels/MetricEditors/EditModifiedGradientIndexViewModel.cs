using PlanScoreCard.Events;
using PlanScoreCard.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.MetricEditors
{
    public class EditModifiedGradientIndexViewModel : BindableBase
    {
        private ScoreMetricModel ScoreMetric;
        private double _doseLow;

        public double DoseLow
        {
            get { return _doseLow; }
            set 
            { 
                SetProperty(ref _doseLow, value);
                ScoreMetric.HI_Lo = DoseLow.ToString();
                _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }
        private double _doseHigh;

        public double DoseHigh
        {
            get { return _doseHigh; }
            set
            {
                SetProperty(ref _doseHigh, value);
                ScoreMetric.HI_Hi = DoseHigh.ToString();
                _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }
        private string _selectedDoseUnitLow;

        public string SelectedDoseUnitLow
        {
            get { return _selectedDoseUnitLow; }
            set
            {
                SetProperty(ref _selectedDoseUnitLow, value);
                ScoreMetric.InputUnit = SelectedDoseUnitLow;
                if (!String.IsNullOrEmpty(SelectedDoseUnitLow))
                {
                    if (SelectedDoseUnitHigh != SelectedDoseUnitLow)
                    {
                        SelectedDoseUnitHigh = SelectedDoseUnitLow;
                    }
                    _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
                }
            }
        }
        private string _selectedDoseUnitHigh;
        private IEventAggregator _eventAggregator;

        public string SelectedDoseUnitHigh
        {
            get { return _selectedDoseUnitHigh; }
            set
            {
                SetProperty(ref _selectedDoseUnitHigh, value);
                if (!String.IsNullOrEmpty(SelectedDoseUnitHigh))
                {
                    if (SelectedDoseUnitLow != SelectedDoseUnitHigh)
                    {
                        SelectedDoseUnitLow = SelectedDoseUnitHigh;
                    }
                    _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
                }
            }
        }
        public List<string> DoseUnits { get; set; }
        public EditModifiedGradientIndexViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DoseUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';').ToList();
            //SelectedDoseUnitLow = DoseUnits.First();
            _eventAggregator.GetEvent<ShowModifiedGradientIndexMetricEvent>().Subscribe(OnShowModifiedGradientIndex);
        }

        private void OnShowModifiedGradientIndex(ScoreMetricModel obj)
        {
            ScoreMetric = obj;
            SelectedDoseUnitHigh = obj.InputUnit;
            DoseLow = Double.TryParse(obj.HI_Lo, out double d) ? Convert.ToDouble(obj.HI_Lo) : 0.0;
            DoseHigh = Double.TryParse(obj.HI_Hi, out double e) ? Convert.ToDouble(obj.HI_Hi) : 0.0;
        }
    }
}
