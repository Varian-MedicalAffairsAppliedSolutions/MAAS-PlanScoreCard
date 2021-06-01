using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;

namespace PlanScoreCard.ViewModels
{
    public class VolumeAtDoseViewModel : BindableBase
	{
		private string _dose;

		public string Dose
		{
			get { return _dose; }
			set { SetProperty(ref _dose, value); }
		}
		private string _selectedVolumeUnit;

		public string SelectedVolumeUnit
		{
			get { return _selectedVolumeUnit; }
			set { SetProperty(ref _selectedVolumeUnit, value); }
		}
		private string _selectedDoseUnit;

		public string SelectedDoseUnit
		{
			get { return _selectedDoseUnit; }
			set { SetProperty(ref _selectedDoseUnit, value); }
		}
		public ObservableCollection<string> DoseUnits { get; set; }
		public ObservableCollection<string> VolumeUnits { get; set; }
		public VolumeAtDoseViewModel()
		{
			DoseUnits = new ObservableCollection<string>();
			VolumeUnits = new ObservableCollection<string>();
			SetUnits();
		}
		/// <summary>
		/// Get the units from the configuration page.
		/// </summary>
		internal void SetUnits()
		{
			var dUnits = ConfigurationManager.AppSettings["DoseUnits"].Split(';');
			var vUnits = ConfigurationManager.AppSettings["VolumeUnits"].Split(';');
			foreach (var du in dUnits)
			{
				DoseUnits.Add(du);
			}
			foreach (var vu in vUnits)
			{
				VolumeUnits.Add(vu);
			}
			SelectedDoseUnit = DoseUnits.FirstOrDefault();
			SelectedVolumeUnit = VolumeUnits.FirstOrDefault();
		}
	}
}
