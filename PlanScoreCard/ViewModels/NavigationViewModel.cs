using Microsoft.Win32;
using Newtonsoft.Json;
using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Models.Proknow;
using PlanScoreCard.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.ViewModels
{
    public class NavigationViewModel : BindableBase
    {

        private string _patientId;
        private string _courseId;
        private string _planId;
        private Application _app;
        private Patient _patient;
        private Course _course;
        private PlanningItem _plan;
        private User _user;
        private IEventAggregator _eventAggregator;

        #region visibilityControls
        private bool _bLocalTemplate;

        public bool bLocalTemplate
        {
            get { return _bLocalTemplate; }
            set { SetProperty(ref _bLocalTemplate, value); }
        }


        private bool _bPKTemplate;

        public bool bPKTemplate
        {
            get { return _bPKTemplate; }
            set { SetProperty(ref _bPKTemplate, value); }
        }
        private bool _bePRTempalte;

        public bool bePRTemplate
        {
            get { return _bePRTempalte; }
            set { SetProperty(ref _bePRTempalte, value); }
        }
        private bool _bTemplateOptions;

        public bool bTemplateOption
        {
            get { return _bTemplateOptions; }
            set { SetProperty(ref _bTemplateOptions, value); }
        }
        public ObservableCollection<string> TemplateOptions { get; set; }
        private List<ScoreTemplateModel> _scoreTemplates;
        private string _selectedPlanDisplay;

        public string SelectedPlanDisplay
        {
            get { return _selectedPlanDisplay; }
            set
            {
                SetProperty(ref _selectedPlanDisplay, value);
            }
        }

        private string selectedTemplateOption;


        public string SelectedTemplateOption
        {
            get { return selectedTemplateOption; }
            set
            {
                SetProperty(ref selectedTemplateOption, value);
                bTemplateOption = false;
                if (SelectedTemplateOption != null)
                {
                    SetVisibilityBasedOnTemplateOption();
                }
            }
        }

        private void SetVisibilityBasedOnTemplateOption()
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (SelectedTemplateOption == "ESAPI PlanScore")
            {
                bLocalTemplate = true;
                bPKTemplate = bePRTemplate = bTemplateOption = false;
                configFile.AppSettings.Settings["ImportTemplate"].Value = "local";
            }
            else if (SelectedTemplateOption == "Proknow")
            {
                bPKTemplate = true;
                bLocalTemplate = bePRTemplate = bTemplateOption = false;
                configFile.AppSettings.Settings["ImportTemplate"].Value = "pk";
            }
            else if (SelectedTemplateOption == "ePeer Review")
            {
                bePRTemplate = true;
                bLocalTemplate = bPKTemplate = bTemplateOption = false;
                configFile.AppSettings.Settings["ImportTemplate"].Value = "ePR";
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.SectionName);
        }

        #endregion visibilityControls
        private PlanModel selectedPlan;
        public PlanModel SelectedPlan
        {
            get { return selectedPlan; }
            set
            {
                SetProperty(ref selectedPlan, value);
                GenerateScorecardCommand.RaiseCanExecuteChanged();
                if (SelectedPlan != null)
                {
                    //Check for the plan in the plansums of the course
                    if (_course.PlanSums != null)
                    {
                        if (_course.PlanSums.FirstOrDefault(x => x.Id == SelectedPlan.PlanId) != null)
                        {
                            _plan = _course.PlanSums.FirstOrDefault(x => x.Id == SelectedPlan.PlanId);
                        }
                    }
                    //check for the plan in the plan setups for the course. 
                    else if (_course.PlanSetups.FirstOrDefault(x => x.Id == SelectedPlan.PlanId) != null)
                    {
                        _plan = _course.PlanSums.FirstOrDefault(x => x.Id == SelectedPlan.PlanId);
                    }

                }
            }
        }
        public ObservableCollection<PlanModel> SelectedPlans { get; private set; }

        public ObservableCollection<PlanModel> Plans { get; private set; }
        public DelegateCommand GenerateScorecardCommand { get; private set; }
        public DelegateCommand ImportScorecardCommand { get; private set; }
        public GenerateScorecardView GenScoreCardView { get; private set; }
        public DelegateCommand ImportPKScorecardCommand { get; private set; }
        public DelegateCommand ImportEPRScorecardCommand { get; private set; }
        public DelegateCommand SetButtonVisibilityCommand { get; private set; }
        public NavigationViewModel(Patient patient, Course course, PlanSetup plan, User user, Application app,
            IEventAggregator eventAggregator)
        {
            _patientId = patient.Id;
            _courseId = course.Id;
            _planId = plan.Id;
            _app = app;
            _patient = patient;
            _course = course;
            _plan = plan;
            _user = user;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<PlanSelectedEvent>().Subscribe(OnPlanSelectionChanged);
            //_eventAggregator.GetEvent<UpdateTemplatesEvent>().Subscribe(OnTemplateUpdated);
            _scoreTemplates = new List<ScoreTemplateModel>();
            Plans = new ObservableCollection<PlanModel>();
            SelectedPlans = new ObservableCollection<PlanModel>();
            GenerateScorecardCommand = new DelegateCommand(OnGenerateScorecard, CanGenerateScorecard);
            ImportScorecardCommand = new DelegateCommand(OnImportScorecard);
            ImportPKScorecardCommand = new DelegateCommand(OnImportPKScorecard);
            ImportEPRScorecardCommand = new DelegateCommand(OnImportEPRScorecard);
            SetButtonVisibilityCommand = new DelegateCommand(OnSetImportVisibilty);
            TemplateOptions = new ObservableCollection<string>();
            TemplateOptions.Add("ESAPI PlanScore");
            TemplateOptions.Add("Proknow");
            TemplateOptions.Add("ePeer Review");
            SetInitialVisibilities();
            SetPlans();
        }
        public void UpdatePlanParameters(Patient patient, Course course, PlanSetup plan, List<string> feedbacks)
        {
            //_app.ClosePatient();
            Plans.Clear();
            _patient = patient;
            _course = course;
            _plan = plan;
            SetPlans();
            foreach (string fb in feedbacks)
            {
                if (fb.Contains("Activate"))
                {
                    Plans.FirstOrDefault(x => x.CourseId == fb.Split(':').Last().Split(';').First().Trim() && x.PlanId == fb.Split(':').Last().Split(';').Last()).bSelected = true;
                    //SelectedPlans.Add(Plans.FirstOrDefault(x => x.CourseId == fb.Split(':').Last().Split(';').First().Trim() && x.PlanId == fb.Split(':').Last().Split(';').Last()));
                }
            }
            SelectedPlanDisplay = String.Join(",", Plans.Where(x => x.bSelected).Select(x => x.PlanId));
            _eventAggregator.GetEvent<PlanChangedEvent>().Publish(SelectedPlans.ToList());
        }

        //public void OnTemplateUpdated(List<ScoreTemplateModel> obj)
        //{
        //    _scoreTemplates = obj;
        //    _eventAggregator.GetEvent<ScorePlanEvent>().Publish(_scoreTemplates);
        //}

        private void OnPlanSelectionChanged(PlanModel obj)
        {
            SelectedPlans.Clear();
            //Plans.Clear();
            SelectedPlanDisplay = String.Join(",", Plans.Where(x => x.bSelected).Select(x => x.PlanId));
            foreach (var plan in Plans)
            {
                if (plan.bSelected)
                {
                    SelectedPlans.Add(plan);
                }
            }
            if(SelectedPlan == null && SelectedPlans.Count() > 0)
            {
                SelectedPlan = SelectedPlans.FirstOrDefault();
            }
            _eventAggregator.GetEvent<PlanChangedEvent>().Publish(SelectedPlans.ToList());
        }

        private void OnSetImportVisibilty()
        {
            //bTemplateOption = true;
            if (!bTemplateOption)
            {
                bTemplateOption = true;
            }
            else { bTemplateOption = false; }
        }
        /// <summary>
        /// Set import visibility options based on last selected item.
        /// </summary>
        private void SetInitialVisibilities()
        {
            switch (ConfigurationManager.AppSettings["ImportTemplate"])
            {
                case "local":
                    SelectedTemplateOption = "ESAPI PlanScore";
                    return;
                case "pk":
                    SelectedTemplateOption = "Proknow";
                    return;
                case "ePR":
                    SelectedTemplateOption = "ePeer Review";
                    return;
                default:
                    SelectedTemplateOption = "ESAPI PlanScore";
                    return;
            }
        }
        /// <summary>
        /// Import ePeer Review Template
        /// </summary>
        private void OnImportEPRScorecard()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CSV (*.csv)|*.csv";
            ofd.Title = "Open ePeerReview Template";
            if (ofd.ShowDialog() == true)
            {
                _scoreTemplates = EPeerReviewScoreModel.GetScoreTemplateFromCSV(ofd.FileName);
                _eventAggregator.GetEvent<ScorePlanEvent>().Publish(_scoreTemplates);
            }
        }

        private void OnImportPKScorecard()
        {
            //ScoreMetrics.Clear();//
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Template (*.json)|*.json";
            ofd.Title = "Open ProKnow template";
            //nt score_newId = 0;// ScoreMetrics.Count();

            if (ofd.ShowDialog() == true)
            {
                PKModel pk_scoreTemplates = JsonConvert.DeserializeObject<PKModel>(File.ReadAllText(ofd.FileName));
                _scoreTemplates = pk_scoreTemplates.ConvertToTemplate();
                _eventAggregator.GetEvent<ScorePlanEvent>().Publish(_scoreTemplates);
            }
        }

        private bool CanGenerateScorecard()
        {
            return SelectedPlan != null;
        }

        private void OnGenerateScorecard()
        {
            GenScoreCardView = new GenerateScorecardView();
            GenScoreCardView.DataContext = new GenerateScorecardViewModel(SelectedPlan, _scoreTemplates, _user, _eventAggregator);
            GenScoreCardView.ShowDialog();
        }

        private void OnImportScorecard()
        {
            //ScoreMetrics.Clear();//
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Template (*.json)|*.json";
            ofd.Title = "Open PlanSC template";
            int score_newId = 0;// ScoreMetrics.Count();

            if (ofd.ShowDialog() == true)
            {
                try
                {
                    InternalTemplateModel template = JsonConvert.DeserializeObject<InternalTemplateModel>(File.ReadAllText(ofd.FileName));
                    _scoreTemplates = template.ScoreTemplates;
                }
                catch
                {
                    try
                    {
                        _scoreTemplates = JsonConvert.DeserializeObject<List<ScoreTemplateModel>>(File.ReadAllText(ofd.FileName));
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(ex.Message);
                    }
                }
                _eventAggregator.GetEvent<ScorePlanEvent>().Publish(_scoreTemplates);
            }
        }

        private void SetPlans()
        {
            if (_course != null)
            {
                AddPlansFromCourse(_course);
                //there have been instances where there are two courses with the same ID on a patient. 
                foreach (var c in _patient.Courses.Where(x => x.Id != _course.Id))
                {
                    AddPlansFromCourse(c);
                }
            }
            if (_plan != null)
            {
                SelectedPlan = Plans.FirstOrDefault(x => x.PlanId == _plan.Id);//i don't think selected plan is doing anything anymore
                if (_plan is PlanSetup)
                {
                    Plans.FirstOrDefault(x => x.CourseId == (_plan as PlanSetup).Course.Id && x.PlanId == _plan.Id).bSelected = true;

                }
                else if (_plan is PlanSum)
                {
                    Plans.FirstOrDefault(x => x.CourseId == (_plan as PlanSum).Course.Id && x.PlanId == _plan.Id).bSelected = true;
                }
                else
                {
                    throw new Exception("Plan cannot be determined as PlanSetup or PlanSum");
                }
                _eventAggregator.GetEvent<PlanChangedEvent>().Publish(Plans.Where(x => x.bSelected).ToList());

            }
        }
        /// <summary>
        /// Adds plans and plansums from the current course. 
        /// </summary>
        /// <param name="course"></param>
        private void AddPlansFromCourse(Course course)
        {
            foreach (var ps in course.PlanSums.Where(x=>x.StructureSet!=null && x.Dose!=null))
            {
                Plans.Add(new PlanModel(ps, _eventAggregator) { PlanId = ps.Id, CourseId = course.Id, DisplayTxt = $"{course.Id}: {ps.Id}" });
            }
            foreach (var ps in course.PlanSetups.Where(x => x.StructureSet != null && x.Dose != null))
            {
                Plans.Add(new PlanModel(ps, _eventAggregator) { PlanId = ps.Id, CourseId = course.Id, DisplayTxt = $"{course.Id}: {ps.Id}" });
            }
        }
        /// <summary>
        /// Method called from MainView when a new template gets saved from the GenerateScoreCardViewModel.
        /// This is necessary because if the plan changes, the scores will be re-calculated based on the new _scoreTemplates object.
        /// </summary>
        /// <param name="templates">The modified template.</param>
        internal void UpdateScoreTemplates(List<ScoreTemplateModel> templates)
        {
            _scoreTemplates = templates;
        }
    }
}
