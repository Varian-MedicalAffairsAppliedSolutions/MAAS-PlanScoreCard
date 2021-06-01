using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels
{
	public class DoseAtVolumeViewModel : BindableBase
	{
		private string _volume;

		public string Volume
		{
			get { return _volume; }
			set { SetProperty(ref _volume, value); }
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
			set
			{
				SetProperty(ref _selectedDoseUnit, value);
			}
		}
		public ObservableCollection<string> DoseUnits { get; set; }
		public ObservableCollection<string> VolumeUnits { get; set; }
		public DoseAtVolumeViewModel()
		{
			DoseUnits = new ObservableCollection<string>();
			VolumeUnits = new ObservableCollection<string>();
			SetUnits();
		}

		private void SetUnits()
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
