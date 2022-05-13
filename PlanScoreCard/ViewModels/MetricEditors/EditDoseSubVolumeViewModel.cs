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
    public class EditDoseSubVolumeViewModel:BindableBase
    {
        private ScoreMetricModel ScoreMetric;
        private string _selectedDoseUnit;

        public string SelectedDoseUnit
        {
            get { return _selectedDoseUnit; }
            set 
            { 
                SetProperty(ref _selectedDoseUnit,value);
                ScoreMetric.OutputUnit = SelectedDoseUnit;
                _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }
        private double _specifiedVolume;
        private IEventAggregator _eventAggregator;

        public double SpecifiedVolume
        {
            get { return _specifiedVolume; }
            set 
            { 
                SetProperty(ref _specifiedVolume,value);
                ScoreMetric.InputValue = SpecifiedVolume.ToString();
                _eventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
            }
        }
        public List<string> DoseUnits { get; set; }
        public EditDoseSubVolumeViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DoseUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';').ToList();
            //SelectedDoseUnit = DoseUnits.First();
            _eventAggregator.GetEvent<ShowDAtSubVMetricEvent>().Subscribe(OnShowDoseAtSubV);
           
        }

        private void OnShowDoseAtSubV(ScoreMetricModel obj)
        {
            ScoreMetric = obj;
            SelectedDoseUnit = obj.OutputUnit;
            SpecifiedVolume = Double.TryParse(obj.InputValue, out double d)?Convert.ToDouble(obj.InputValue):0.0;
        }
    }
}
