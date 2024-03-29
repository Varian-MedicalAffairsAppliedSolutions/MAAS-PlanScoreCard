﻿using Microsoft.Win32;
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
        private ScoreCardModel _scoreCard;
        private List<PlanModel> _plans;

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
                RemovePatientCommand.RaiseCanExecuteChanged();
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
                OpenPatientCommand.RaiseCanExecuteChanged();
            }
        }
        private string _scoreCardUpdateText;

        public string ScoreCardUpdateText
        {
            get { return _scoreCardUpdateText; }
            set
            {
                SetProperty(ref _scoreCardUpdateText, value);
            }
        }

        public DelegateCommand SearchPatientCommand { get; private set; }
        public DelegateCommand OpenPatientCommand { get; private set; }
        public DelegateCommand PatientImportCommand { get; private set; }
        public DelegateCommand SavePlansCommand { get; private set; }
        public DelegateCommand CancelPlansCommand { get; private set; }
        public DelegateCommand SavePatientListCommand { get; private set; }
        public DelegateCommand RemovePatientCommand { get; private set; }
        public PatientSelectService PatientSelectService { get; }

        public PatientSelectionViewModel(IEventAggregator eventAggregator, Application app, List<PlanModel> plans, ScoreCardModel scoreCard)
        {
            _eventAggregator = eventAggregator;
            _app = app;
            _scoreCard = scoreCard;
            _plans = plans;
            Patients = new ObservableCollection<PatientPlanSearchModel>();
            PatientMatches = new ObservableCollection<PatientSelectModel>();
            SavePlansCommand = new DelegateCommand(OnSavePlans);
            CancelPlansCommand = new DelegateCommand(OnCancelPlans);
            PatientSummaries = new List<PatientSummaryModel>();
            OpenPatientCommand = new DelegateCommand(OnOpenPatient, CanOpenPatient);
            PatientImportCommand = new DelegateCommand(OnImportPatients);
            SavePatientListCommand = new DelegateCommand(OnSavePatientList);
            RemovePatientCommand = new DelegateCommand(OnClearPatientList, CanRemovePatient);
            SetScoreCardText();
            GetPatientSummaryies();
            PatientSelectService = new PatientSelectService(new SmartSearchService(PatientSummaries));
            SearchPatient();
            //TODO add patient smart search -- can add from redcurry example
            //https://github.com/redcurry/EsapiEssentials
            //smartSearch = new SmartSearchService(Patients);
            //Patients = new SmartSearchService(app.PatientSummaries).GetMatchingPatients(AddPatientId);
            SetPatientsInitial();
            // SearchText = String.Empty;
            _eventAggregator.GetEvent<FreePrimarySelectionEvent>().Subscribe(OnResetPrimaryPlan);
            _eventAggregator.GetEvent<UpdateTemplateMatchesEvent>().Subscribe(OnUpdateTemplateMatches);
            _eventAggregator.GetEvent<EvaluateStructureMatchesEvent>().Subscribe(OnEvaluateStructureMatches);
        }

        private void SetPatientsInitial()
        {
            Patients.Clear();
            foreach (var patient in _plans.GroupBy(p => p.PatientId))
            {
                _app.ClosePatient();
                var localPatient = _app.OpenPatientById(patient.Key);
                var localPatientSelectModel = new PatientPlanSearchModel(localPatient, _eventAggregator);
                List<StructureModel> localStructureList = new List<StructureModel>();
                foreach (var plan in patient)
                {
                    var localPlan = localPatientSelectModel.Plans.FirstOrDefault(pl => pl.PatientId == plan.PatientId
                    && pl.CourseId == plan.CourseId
                    && pl.PlanId == plan.PlanId);
                    localPlan.bSelected = plan.bSelected;
                    localPlan.bPrimary = plan.bPrimary;
                    if (_plans.Any(pl => pl.PlanId == localPlan.PlanId && pl.CourseId == localPlan.CourseId && pl.PatientId == localPlan.PatientId))
                    {
                        var inputPlan = _plans.FirstOrDefault(pl => pl.PlanId == localPlan.PlanId && pl.CourseId == localPlan.CourseId && pl.PatientId == localPlan.PatientId);
                        if (inputPlan.TemplateStructures.Any())
                        {//in case the patient has already been through the matching process.
                            localPlan.EvaluateStructureMatches(inputPlan.TemplateStructures.ToList());
                        }
                    }
                }
                if (_scoreCard != null)
                {
                    if (!patient.Any(p => p.TemplateStructures.Any()))//patient has not been through matching process.
                    {
                        localPatientSelectModel.EvaluateStructureMatches(_scoreCard.ScoreMetrics.ToList().Select(sm => sm.Structure).ToList());
                    }
                    else//evaluate flags for patients that have been through matching process.
                    {
                        foreach (var plan in localPatientSelectModel.Plans.Where(p => !p.TemplateStructures.Any()))
                        {
                            plan.EvaluateStructureMatches(_scoreCard.ScoreMetrics.ToList().Select(sm => sm.Structure).ToList());
                        }
                        localPatientSelectModel.EvaluateFlags();
                    }
                }
                Patients.Add(localPatientSelectModel);
            }
        }

        private void OnEvaluateStructureMatches()
        {
            string localPatientId = SelectedPatient.PatientId;
            string localCourseId = SelectedPatient.SelectedPlan.CourseId;
            string localPlanId = SelectedPatient.SelectedPlan.PlanId;
            SelectedPatient = null;
            Patients.Clear();
            SetPatientsInitial();
            SelectedPatient = Patients.FirstOrDefault(p => p.PatientId.Equals(localPatientId));
            SelectedPatient.SelectedPlan = SelectedPatient.Plans.FirstOrDefault(pl => pl.CourseId.Equals(localCourseId) && pl.PlanId.Equals(localPlanId));

        }

        //when matching a template structure you can copy to all patients. 
        private void OnUpdateTemplateMatches(StructureModel obj)
        {
            string currentPatientId = SelectedPatient.PatientId;
            SelectedPatient = null;
            foreach (var patient in Patients)
            {
                foreach (var plan in patient.Plans)
                {
                    var templateStructure = plan.TemplateStructures.FirstOrDefault(ts => ts.TemplateStructureInt == obj.TemplateStructureInt);
                    if (templateStructure != null)
                    {
                        if (plan.Structures.Any(s => s.StructureId == obj.MatchedStructure.StructureId))
                        {
                            //because of the way MatchedStructure is set up, this will update all the hidden items as well. 
                            templateStructure.MatchedStructure = plan.Structures.FirstOrDefault(s => s.StructureId == obj.MatchedStructure.StructureId);
                        }
                    }
                }
            }
            SelectedPatient = Patients.FirstOrDefault(p => p.PatientId == currentPatientId);
            SelectedPatient.EvaluateFlags();
        }

        private void SetScoreCardText()
        {
            if (_scoreCard == null)
            {
                ScoreCardUpdateText = $"Cannot compare patients to scorecards. No metrics detected.";
            }
            else if (_scoreCard.ScoreMetrics.Count() == 0)
            {
                ScoreCardUpdateText = "Cannot compare patients to scorecards. No metrics detected.";
            }
            else
            {
                ScoreCardUpdateText = $"Scorecard imported with {_scoreCard.ScoreMetrics.Count()} metrics";
            }
        }

        private bool CanRemovePatient()
        {
            return SelectedPatient != null;
        }

        private bool CanOpenPatient()
        {
            return !String.IsNullOrEmpty(SearchText);
        }

        private void OnClearPatientList()
        {
            if (SelectedPatient != null)
            {
                string patientId = SelectedPatient.PatientId;
                if (SelectedPatient.SelectedPlan != null)
                {
                    SelectedPatient.SelectedPlan.Structures.Clear();
                    SelectedPatient.SelectedPlan = null;
                }
                _plans = _plans.Where(pl => pl.PatientId != SelectedPatient.PatientId).ToList();
                SelectedPatient.Plans.Clear();
                SelectedPatient = null;

                Patients.Remove(Patients.FirstOrDefault(p => p.PatientId.Equals(patientId)));
                SetPatientsInitial();
            }
            //if (SelectedPatient != null)
            //{
            //    foreach (var plan in _plans.Where(p => p.PatientId.Equals(SelectedPatient.PatientId)))
            //    {
            //        _plans.Remove(plan);
            //    }
            //    //Patients.Remove(SelectedPatient);
            //    SelectedPatient = null;
            //    SetPatientsInitial();
            //    //Patients.Clear();
            //    //SearchText = String.Empty;
            //}
        }

        private void OnSavePatientList()
        {
            List<PatientPlanModel> localPatientPlanModel = new List<PatientPlanModel>();
            foreach (var patient in Patients)
            {
                foreach (var plan in patient.Plans)
                {
                    if (plan.bSelected)
                    {
                        localPatientPlanModel.Add(new PatientPlanModel
                        {
                            PatientId = plan.PatientId,
                            CourseId = plan.CourseId,
                            PlanId = plan.PlanId
                        });
                    }
                }
            }
            SaveFileDialog saver = new SaveFileDialog();
            saver.Filter = "JSON Files (*.json)|*.json";
            if (saver.ShowDialog() == true)
            {
                File.WriteAllText(saver.FileName, JsonConvert.SerializeObject(localPatientPlanModel));
            }
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
            ofd.Filter = "JSON File (*.json)|*.json|CSV File (*.csv)|*.csv";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    //import logic for CSV
                    List<PatientPlanModel> patients = new List<PatientPlanModel>();
                    if (ofd.FileName.EndsWith("csv"))
                    {
                        foreach (var line in File.ReadAllLines(ofd.FileName))
                        {
                            patients.Add(new PatientPlanModel
                            {
                                PatientId = line.Split(',').First(),
                                CourseId = line.Split(',').ElementAt(1),
                                PlanId = line.Split(',').Last()
                            });
                            _app.ClosePatient();
                            var patient = _app.OpenPatientById(line.Split(',').First());
                            if (patient != null)
                            {
                                var course = patient.Courses.FirstOrDefault(c => c.Id.Equals(line.Split(',').ElementAt(1)));
                                if (course != null)
                                {
                                    var planSetup = course.PlanSetups.FirstOrDefault(ps => ps.Id.Equals(line.Split(',').Last()));
                                    if (planSetup != null)
                                    {
                                        _plans.Add(new PlanModel(planSetup, _eventAggregator));
                                    }
                                }
                            }
                        }
                    }//import logic for json
                    else
                    {
                        patients = JsonConvert.DeserializeObject<List<PatientPlanModel>>(File.ReadAllText(ofd.FileName));
                        foreach (var patient in patients)
                        {
                            _app.ClosePatient();
                            var pat = _app.OpenPatientById(patient.PatientId);
                            if (pat != null)
                            {
                                var course = pat.Courses.FirstOrDefault(c => c.Id == patient.CourseId);
                                if (course != null)
                                {
                                    var planSetup = course.PlanSetups.FirstOrDefault(ps => ps.Id == patient.PlanId);
                                    if (planSetup != null)
                                    {
                                        _plans.Add(new PlanModel(planSetup, _eventAggregator));
                                    }
                                }
                            }
                        }
                    }
                    //all patient and plans are here. 
                    foreach (var patient in patients)
                    {
                        if (!Patients.Any(x => x.PatientId == patient.PatientId))
                        {
                            SearchText = patient.PatientId;
                            OnOpenPatient();
                        }
                        if (_plans.Any(p => p.PatientId == patient.PatientId))
                        {
                            if (_plans.Any(pl => pl.PatientId == patient.PatientId && pl.CourseId == patient.CourseId && pl.PlanId == patient.PlanId))
                            {
                                _plans.FirstOrDefault(p => p.PatientId == patient.PatientId && p.CourseId == patient.CourseId && p.PlanId == patient.PlanId).bSelected = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Could not parse file {ofd.FileName}\n{ex.Message}");
                }
                SetPatientsInitial();
            }
        }

        private void OnOpenPatient()
        {

            if (!Patients.Any(p => p.PatientId == SearchText) && !String.IsNullOrEmpty(SearchText))//should not be able to open the same patient twice.
            {
                _app.ClosePatient();
                var patient = _app.OpenPatientById(SearchText);
                if (patient != null)
                {
                    //Patients.Add(new PatientPlanSearchModel(patient, _eventAggregator));
                    foreach (var course in patient.Courses)
                    {
                        foreach (var plan in course.PlanSetups.Where(ps => ps.StructureSet != null))
                        {
                            _plans.Add(new PlanModel(plan, _eventAggregator));
                        }
                    }
                    SetPatientsInitial();
                }
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
