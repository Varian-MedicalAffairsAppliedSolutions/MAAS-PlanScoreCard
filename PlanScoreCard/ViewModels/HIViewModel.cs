using PlanScoreCard.ViewModels.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;

namespace PlanScoreCard.ViewModels
{
    public class HIViewModel : BindableBase , IScoreMetricViewModel
	{
		private double _hi_highValue;

		public double HI_HiValue
		{
			get { return _hi_highValue; }
			set { SetProperty(ref _hi_highValue, value); }
		}
		private double _hi_lowValue;

		public double HI_LowValue
		{
			get { return _hi_lowValue; }
			set { SetProperty(ref _hi_lowValue, value); }
		}
		private double _targetValue;

		public double TargetValue
		{
			get { return _targetValue; }
			set { SetProperty(ref _targetValue, value); }
		}
		private string _selectedDoseUnit;
		public string SelectedDoseUnit
		{
			get { return _selectedDoseUnit; }
			set
			{
				SetProperty(ref _selectedDoseUnit, value);
			}
		}
		public ObservableCollection<string> DoseUnits { get; set; }

		public HIViewModel()
		{
			HI_HiValue = Convert.ToDouble(ConfigurationManager.AppSettings["HI_HI"]);
			HI_LowValue = Convert.ToDouble(ConfigurationManager.AppSettings["HI_LO"]);
			DoseUnits = new ObservableCollection<string>();
			var dUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';');
			foreach (var du in dUnits)
			{
				DoseUnits.Add(du);
			}
			if (DoseUnits.Contains("%")) { DoseUnits.Remove("%"); }
			SelectedDoseUnit = DoseUnits.FirstOrDefault();
		}

	}
}
