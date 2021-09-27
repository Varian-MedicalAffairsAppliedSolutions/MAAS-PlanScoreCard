using PlanScoreCard.ViewModels.Interfaces;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;

namespace PlanScoreCard.ViewModels
{
    public class DoseValueViewModel : BindableBase , IScoreMetricViewModel
	{
		public ObservableCollection<string> DoseUnits { get; private set; }
		private string _selectedDoseUnit;

		public string SelectedDoseUnit
		{
			get { return _selectedDoseUnit; }
			set { SetProperty(ref _selectedDoseUnit, value); }
		}
		public DoseValueViewModel()
		{
			DoseUnits = new ObservableCollection<string>();
			SetDoseUnits();
		}

		private void SetDoseUnits()
		{
			string[] doseUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';');
			foreach (var dU in doseUnits)
			{
				DoseUnits.Add(dU);
			}
			SelectedDoseUnit = DoseUnits.FirstOrDefault();
		}
	}
}
