using PlanScoreCard.Models;
using PlanScoreCard.Services;
using Prism.Commands;
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
    public class PatientSelectionViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator;
        private Application _app;

        public ObservableCollection<PatientPlanSearchModel> Patients { get; private set; }
        public List<PatientSummaryModel> PatientSummaries { get; private set; }
        public ObservableCollection<PatientSelectModel> PatientMatches { get; private set; }

        //private SmartSearchService smartSearch;
        private PatientPlanSearchModel _selectedPatient;

        public PatientPlanSearchModel SelectedPatient
        {

            get { return _selectedPatient; }
            set 
            { 
                SetProperty(ref _selectedPatient, value); 
            }
        }
        private PatientSelectModel _selectedPatientMatch;

        public PatientSelectModel SelectedPatientMatch
        {
            get { return _selectedPatientMatch; }
            set 
            { 
                _selectedPatientMatch = value;
                if (SelectedPatientMatch!=null)
                {
                    SearchText = SelectedPatientMatch.ID;
                }
            }
        }

        private string _searchText;

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                SetProperty(ref _searchText, value);
                SearchPatient();
            }
        }
        public DelegateCommand SearchPatientCommand { get; private set; }
        public DelegateCommand OpenPatientCommand { get; private set; }
        public PatientSelectService PatientSelectService { get; }

        public PatientSelectionViewModel(IEventAggregator eventAggregator, Application app)
        {
            _eventAggregator = eventAggregator;
            _app = app;
            Patients = new ObservableCollection<PatientPlanSearchModel>();
            PatientMatches = new ObservableCollection<PatientSelectModel>();
            PatientSummaries = new List<PatientSummaryModel>();
            OpenPatientCommand = new DelegateCommand(OnOpenPatient);
            GetPatientSummaryies();
            PatientSelectService = new PatientSelectService(new SmartSearchService(PatientSummaries));
            SearchPatient();
            //TODO add patient smart search -- can add from redcurry example
            //https://github.com/redcurry/EsapiEssentials
            //smartSearch = new SmartSearchService(Patients);
            //Patients = new SmartSearchService(app.PatientSummaries).GetMatchingPatients(AddPatientId);
        }

        private void OnOpenPatient()
        {
            _app.ClosePatient();
            var patient = _app.OpenPatientById(SearchText);
            Patients.Add(new PatientPlanSearchModel(patient, _eventAggregator));
        }

        private void GetPatientSummaryies()
        {
            foreach (var patientSummary in _app.PatientSummaries)
            {
                PatientSummaries.Add(new PatientSummaryModel(patientSummary));
            }
        }

        private void SearchPatient()
        {
            PatientMatches.Clear();
            foreach(var ps in PatientSelectService.GetPatientOptions(SearchText))
            {
                PatientMatches.Add(ps);
            }
        }
    }
}
