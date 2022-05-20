using PlanScoreCard.Events;
using PlanScoreCard.Models;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.MetricEditors
{
    public class EditHIViewModel : BindableBase
    {
		private ScoreMetricModel ScoreMetric;

		private double _hi_highValue;

		public double HI_HiValue
		{
			get { return _hi_highValue; }
			set 
			{ 
				SetProperty(ref _hi_highValue, value);
				ScoreMetric.HI_Hi = HI_HiValue.ToString();
				EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
			}
		}
		private double _hi_lowValue;

		public double HI_LowValue
		{
			get { return _hi_lowValue; }
			set 
			{ 
				SetProperty(ref _hi_lowValue, value);
				ScoreMetric.HI_Lo = HI_LowValue.ToString();
				EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
			}
		}
		private double _targetValue;

		public double TargetValue
		{
			get { return _targetValue; }
			set 
			{ 
				SetProperty(ref _targetValue, value);
				ScoreMetric.HI_Target = TargetValue.ToString();
				EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
			}
		}
		private string _selectedDoseUnit;
		public string SelectedDoseUnit
		{
			get { return _selectedDoseUnit; }
			set
			{
				SetProperty(ref _selectedDoseUnit, value);
				ScoreMetric.InputUnit = SelectedDoseUnit;
				EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Publish();
			}
		}
		public ObservableCollection<string> DoseUnits { get; set; }

		private IEventAggregator EventAggregator;
		public EditHIViewModel(IEventAggregator eventAggregator)
		{
			EventAggregator = eventAggregator;
			EventAggregator.GetEvent<ShowHIMetricEvent>().Subscribe(SetMetric);

			DoseUnits = new ObservableCollection<string>(); 

			var dUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';');
			var vUnits = ConfigurationManager.AppSettings["VolumeUnits"].Split(';');
			foreach (var du in dUnits)
			{
				DoseUnits.Add(du);
			}
		}

        private void SetMetric(ScoreMetricModel scoreMetric)
        {
			ScoreMetric = scoreMetric;
			SelectedDoseUnit = scoreMetric.InputUnit;
			HI_HiValue = Convert.ToDouble(scoreMetric.HI_Hi);
			HI_LowValue = Convert.ToDouble(scoreMetric.HI_Lo);
			TargetValue = Convert.ToDouble(scoreMetric.HI_Target);
		}
    }
}
