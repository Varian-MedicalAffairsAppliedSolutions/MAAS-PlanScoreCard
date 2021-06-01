using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;

namespace PlanScoreCard.ViewModels
{
    public class CIViewModel : BindableBase
	{
		private string _dose;

		public string Dose
		{
			get { return _dose; }
			set { SetProperty(ref _dose, value); }
		}
		private string _selectedDoseUnit;

		public string SelectedDoseUnit
		{
			get { return _selectedDoseUnit; }
			set { SetProperty(ref _selectedDoseUnit, value); }
		}
		public ObservableCollection<string> DoseUnits { get; set; }
		public CIViewModel()
		{
			DoseUnits = new ObservableCollection<string>();
			SetUnits();
		}
		private void SetUnits()
		{
			var dUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';');
			foreach (var du in dUnits)
			{
				DoseUnits.Add(du);
			}
			SelectedDoseUnit = DoseUnits.FirstOrDefault();
		}
	}
}
