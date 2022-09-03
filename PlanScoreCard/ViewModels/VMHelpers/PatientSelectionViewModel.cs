using PlanScoreCard.Models;
using PlanScoreCard.Services;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class PatientSelectionViewModel:BindableBase
    {
        private IEventAggregator _eventAggregator;
        private Application _app;

        public ObservableCollection<PatientSummaryModel> Patients { get; private set; }

        //private SmartSearchService smartSearch;
        private PatientSummaryModel _selectedPatient;

        public PatientSummaryModel SelectedPatient
        {
            get { return _selectedPatient; }
            set { SetProperty(ref _selectedPatient,value); }
        }
        private string _addPatientId;

        public string AddPatientId
        {
            get { return _addPatientId; }
            set { 
                SetProperty(ref _addPatientId,value);
                UpdatePatientsOnId();
            }
        }


        public PatientSelectionViewModel(IEventAggregator eventAggregator, Application app)
        {
            _eventAggregator = eventAggregator;
            _app = app;
            Patients = new ObservableCollection<PatientSummaryModel>();
            GetPatientSummaryies();
            //TODO add patient smart search -- can add from redcurry example
            //https://github.com/redcurry/EsapiEssentials
            //smartSearch = new SmartSearchService(Patients);
            //Patients = new SmartSearchService(app.PatientSummaries).GetMatchingPatients(AddPatientId);
        }

        private void GetPatientSummaryies()
        {
            foreach(var patientSummary in _app.PatientSummaries)
            {
                Patients.Add(new PatientSummaryModel(patientSummary));
            }
        }

        private void UpdatePatientsOnId()
        {
            throw new NotImplementedException();
        }
    }
}
