using Microsoft.Win32;
using Newtonsoft.Json;
using PlanScoreCard.Events;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
                if (SelectedPatientMatch != null)
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
        public DelegateCommand PatientImportCommand { get; private set; }
        public DelegateCommand SavePlansCommand { get; private set; }
        public DelegateCommand CancelPlansCommand { get; private set; }
        public PatientSelectService PatientSelectService { get; }

        public PatientSelectionViewModel(IEventAggregator eventAggregator, Application app, List<PlanModel> plans)
        {
            _eventAggregator = eventAggregator;
            _app = app;
            Patients = new ObservableCollection<PatientPlanSearchModel>();
            PatientMatches = new ObservableCollection<PatientSelectModel>();
            SavePlansCommand = new DelegateCommand(OnSavePlans);
            CancelPlansCommand = new DelegateCommand(OnCancelPlans);
            PatientSummaries = new List<PatientSummaryModel>();
            OpenPatientCommand = new DelegateCommand(OnOpenPatient);
            PatientImportCommand = new DelegateCommand(OnImportPatients);
            GetPatientSummaryies();
            PatientSelectService = new PatientSelectService(new SmartSearchService(PatientSummaries));
            SearchPatient();
            //TODO add patient smart search -- can add from redcurry example
            //https://github.com/redcurry/EsapiEssentials
            //smartSearch = new SmartSearchService(Patients);
            //Patients = new SmartSearchService(app.PatientSummaries).GetMatchingPatients(AddPatientId);
            foreach (var patient in plans.GroupBy(p => p.PatientId))
            {
                _app.ClosePatient();
                var localPatient = _app.OpenPatientById(patient.Key);
                var localPatientSelectModel = new PatientPlanSearchModel(localPatient, eventAggregator);

                foreach (var plan in patient)
                {
                    var localPlan = localPatientSelectModel.Plans.FirstOrDefault(pl => pl.PatientId == plan.PatientId
                    && pl.CourseId == plan.CourseId
                    && pl.PlanId == plan.PlanId);
                    localPlan.bSelected = plan.bSelected;
                    localPlan.bPrimary = plan.bPrimary;
                }
                Patients.Add(localPatientSelectModel);
            }
           // SearchText = String.Empty;
            _eventAggregator.GetEvent<FreePrimarySelectionEvent>().Subscribe(OnResetPrimaryPlan);
        }

        private void OnResetPrimaryPlan(PlanModel obj)
        {
            if (obj.bPrimary)
            {
                foreach (var patient in Patients)
                {
                    foreach (var plan in patient.Plans)
                    {
                        if (plan != obj)
                        {
                            plan.bPrimary = false;
                        }
                    }
                }
            }
        }
        private bool _isSaved;
        private void OnCancelPlans()
        {
            _eventAggregator.GetEvent<ClosePatientSelectionEvent>().Publish(_isSaved);
            //_isSaved = false;
        }

        private void OnSavePlans()
        {
            _eventAggregator.GetEvent<UpdatePatientPlansEvent>().Publish(Patients.ToList());
            _isSaved = true;
            OnCancelPlans();
        }

        private void OnImportPatients()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV File (*.csv)|*.csv|JSON File (*.json)|*.json";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    List<PatientPlanModel> patients = new List<PatientPlanModel>();
                    if (ofd.FileName.EndsWith("csv"))
                    {
                        foreach(var line in File.ReadAllLines(ofd.FileName))
                        {
                            patients.Add(new PatientPlanModel
                            {
                                PatientId = line.Split(',').First(),
                                CourseId = line.Split(',').ElementAt(1),
                                PlanId = line.Split(',').Last()
                            });
                        }
                    }
                    else
                    {
                        patients = JsonConvert.DeserializeObject<List<PatientPlanModel>>(File.ReadAllText(ofd.FileName));
                    }
                    foreach (var patient in patients)
                    {
                        if (!Patients.Any(x => x.PatientId == patient.PatientId))
                        {
                            SearchText = patient.PatientId;
                            OnOpenPatient();
                        }
                        if (Patients.Any(p => p.PatientId == patient.PatientId))
                        {
                            if (Patients.First(p => p.PatientId == patient.PatientId).Plans.Any(pl => pl.CourseId == patient.CourseId && pl.PlanId == patient.PlanId))
                            {
                                Patients.FirstOrDefault(p => p.PatientId == patient.PatientId).Plans.FirstOrDefault(p => p.CourseId == patient.CourseId && p.PlanId == patient.PlanId).bSelected = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Could not parse file {ofd.FileName}\n{ex.Message}");
                }
            }
        }

        private void OnOpenPatient()
        {
            if (!Patients.Any(p => p.PatientId == SearchText))//should not be able to open the same patient twice.
            {
                _app.ClosePatient();
                var patient = _app.OpenPatientById(SearchText);
                Patients.Add(new PatientPlanSearchModel(patient, _eventAggregator));
            }
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
            foreach (var ps in PatientSelectService.GetPatientOptions(SearchText))
            {
                PatientMatches.Add(ps);
            }
        }
    }
}
