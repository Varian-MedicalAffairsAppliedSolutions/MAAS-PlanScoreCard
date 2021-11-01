using Microsoft.Win32;
using Newtonsoft.Json;
using PlanScoreCard.Events;
using PlanScoreCard.Events.Plugin;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Models.Proknow;
using PlanScoreCard.Services;
using PlanScoreCard.Views;
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

namespace PlanScoreCard.ViewModels
{
    public class ScoreCardViewModel : BindableBase
    {

        // Class Variables 

        private Application Application;
        private Patient Patient;
        private Course Course;
        private PlanningItem Plan;
        private IEventAggregator EventAggregator;
        private ViewLauncherService ViewLauncherService;

        private ScoreCardModel scoreCard;
        public ScoreCardModel ScoreCard
        {
            get { return scoreCard; }
            set { SetProperty(ref scoreCard, value); }
        }

        private List<ScoreTemplateModel> ScoreTemplates;
        private string TemplateName;
        private string TemplateSite;
        
        // Full Properties

        private string scoreTotalText;
        public string ScoreTotalText
        {
            get { return scoreTotalText; }
            set { SetProperty(ref scoreTotalText, value); }
        }
        
        private bool bpluginVisibility;
        public bool bPluginVisibility
        {
            get { return bpluginVisibility; }
            set { SetProperty(ref bpluginVisibility, value); }
        }
        
        private int pluginWidth;
        public int PluginWidth
        {
            get { return pluginWidth; }
            set { SetProperty(ref pluginWidth, value); }
        }

        private string patientId;

        public string PatientId
        {
            get { return patientId; }
            set { SetProperty(ref patientId , value); }
        }

        private PlanModel selectedPlan;

        public PlanModel SelectedPlan
        {
            get { return selectedPlan; }
            set { SetProperty(ref selectedPlan , value); }
        }

        private string scoreCardName;

        public string ScoreCardName
        {
            get { return scoreCardName; }
            set { SetProperty(ref scoreCardName, value); }
        }

        // Observable Collections
        private ObservableCollection<PlanScoreModel> planScores;

        public ObservableCollection<PlanScoreModel> PlanScores
        {
            get { return planScores; }
            set { SetProperty( ref planScores , value); }
        }

        private ObservableCollection<PlanModel> plans;

        public ObservableCollection<PlanModel> Plans
        {
            get { return plans; }
            set { SetProperty(ref plans , value); }
        }

        // Delegate Commands
        public DelegateCommand ScorePlanCommand { get; set;  }
        public DelegateCommand ImportScoreCardCommand { get; set; }
        public DelegateCommand EditScoreCardCommand { get; set; }
        public DelegateCommand NormalizePlanCommand { get; set; }
        public DelegateCommand ExportScoreCardCommand { get; set; }

        // Constructor
        public ScoreCardViewModel(Application app, Patient patient, Course course, PlanSetup plan, IEventAggregator eventAggregator, ViewLauncherService viewLauncherService)
        {
            // Set the Initial Variables Passed In
            Application  = app;
            Patient = patient;
            Course = course;
            Plan = plan;
            EventAggregator = eventAggregator;

            // Initiate Service
            ViewLauncherService = viewLauncherService;

            // Need to change this event to take in a ScoreCardModel as the payload
            //EventAggregator.GetEvent<ScorePlanEvent>().Subscribe(OnScorePlan);
            EventAggregator.GetEvent<PluginVisibilityEvent>().Subscribe(OnPluginVisible);
            EventAggregator.GetEvent<PlanChangedEvent>().Subscribe(OnPlanChanged);
            EventAggregator.GetEvent<ScorePlanEvent>().Subscribe(ScorePlan);
            EventAggregator.GetEvent<PlanSelectedEvent>().Subscribe(ScorePlan);
            
            // Initiate Collections
            PlanScores = new ObservableCollection<PlanScoreModel>();
            Plans = new ObservableCollection<PlanModel>();

            // Delegate Commands
            ScorePlanCommand = new DelegateCommand(ScorePlan);
            ImportScoreCardCommand = new DelegateCommand(ImportScoreCard);
            EditScoreCardCommand = new DelegateCommand(EditScoreCard);

            // Sets If no Plan is Passed In
            if (Plan != null)
                OnPlanChanged(new List<PlanModel> { new PlanModel(Plan as PlanningItem, eventAggregator) { PlanId = Plan.Id, CourseId = Course.Id, bSelected = true } });

            InitializeClass();
        }

        // Initialize 
        private void InitializeClass()
        {

            // Set the PatientID
            PatientId = Patient.Id;

            // Add the Plans
            // Clear the Collection of Plans
            Plans.Clear();

            // For each course, add all the Plans
            foreach (Course course in Patient.Courses)
            {
                foreach (PlanSetup plan in course.PlanSetups)
                    Plans.Add(new PlanModel(plan, EventAggregator));
            }

            Plans.First().bPrimary = true;

        }

        private void ScorePlan()
        {
            if (ScoreCard !=  null)
                ScorePlan(ScoreCard);

        }

        private void EditScoreCard()
        {
            ScoreCardModel scoreCard = new ScoreCardModel(TemplateName, TemplateSite, ScoreTemplates);
            
            EditScoreCardView editScoreCardView = ViewLauncherService.GetEditScoreCardView();

            EventAggregator.GetEvent<EditScoreCardSetPlanEvent>().Publish(new PlanModel(Plan, EventAggregator)); // Push the SelectedPlan
            EventAggregator.GetEvent<LoadEditScoreCardViewEvent>().Publish(scoreCard); // Push the ScoreCardModel to the ViewModel
            EventAggregator.GetEvent<EditScoreCardSetUserEvent>().Publish(Application.CurrentUser); // Push the User

            editScoreCardView.ShowDialog();
        }

        private void ImportScoreCard()
        {
            //ScoreMetrics.Clear();//
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON Template (*.json)|*.json|ePeer Review(*.csv)|*.csv";
            ofd.Title = "Open Planscore Template";
            int score_newId = 0;// ScoreMetrics.Count();
            bool importSuccess = false;


            if (ofd.ShowDialog() == true)
            {

                if (!String.IsNullOrEmpty(ofd.FileName))
                    ScoreCardName = ofd.SafeFileName;

                if (ofd.FileName.EndsWith(".csv"))
                {
                    try
                    {
                        ScoreTemplates = EPeerReviewScoreModel.GetScoreTemplateFromCSV(ofd.FileName);
                        importSuccess = true;
                    }
                    catch
                    {
                        importSuccess = false;
                    }
                }
                else
                {
                    try
                    {
                        InternalTemplateModel template = JsonConvert.DeserializeObject<InternalTemplateModel>(File.ReadAllText(ofd.FileName));
                        TemplateName = template.TemplateName;
                        TemplateSite = template.Site;
                        ScoreTemplates = template.ScoreTemplates;
                        if (ScoreTemplates.Count() == 0)
                        {
                            PKModel pk_scoreTemplates = JsonConvert.DeserializeObject<PKModel>(File.ReadAllText(ofd.FileName));
                            ScoreTemplates = pk_scoreTemplates.ConvertToTemplate();
                        }
                        importSuccess = true;
                    }
                    catch
                    {
                        try
                        {
                            ScoreTemplates = JsonConvert.DeserializeObject<List<ScoreTemplateModel>>(File.ReadAllText(ofd.FileName));
                            importSuccess = true;
                        }
                        catch
                        {
                            try
                            {
                                PKModel pk_scoreTemplates = JsonConvert.DeserializeObject<PKModel>(File.ReadAllText(ofd.FileName));
                                ScoreTemplates = pk_scoreTemplates.ConvertToTemplate();

                                importSuccess = true;
                            }
                            catch (Exception ex)
                            {
                                throw new ApplicationException(ex.Message);
                            }
                        }
                    }


                    ScoreCard = new ScoreCardModel(TemplateName,TemplateSite,ScoreTemplates);

                }
                if (importSuccess)
                {
                    EventAggregator.GetEvent<ScorePlanEvent>().Publish(ScoreCard);
                }
            }
        }

        public void ScorePlan(ScoreCardModel scoreCard)
        {
            // _eventAggregator.GetEvent<UpdateTemplatesEvent>().Publish(_currentTemplate);
            PlanScores.Clear();

            // Get Collection of SelectedPlans
            List<PlanningItem> selectedPlans = Plans.Where(p => p.bSelected == true).Select(s => s.Plan as PlanningItem).ToList();
            ObservableCollection<PlanningItem> selectedPlanCollection = new ObservableCollection<PlanningItem>();
            foreach (PlanningItem plan in selectedPlans)
                selectedPlanCollection.Add(plan);

            int metric_id = 0;
            foreach (var template in scoreCard.ScoreMetrics)
            {
                var psm = new PlanScoreModel(Application);
                psm.BuildPlanScoreFromTemplate(selectedPlanCollection, template, metric_id);
                PlanScores.Add(psm);
                metric_id++;
            }

            //remove score points from metrics that didn't have the
            if (PlanScores.Any(x => x.ScoreValues.Count() > 0))
            {
                var planScores = PlanScores.Where(x => x.ScoreValues.First().Value > -999);
                ScoreTotalText = $"Plan Scores: ";
                if (planScores.Count() != 0)
                {
                    foreach (var pc in planScores.FirstOrDefault().ScoreValues.Select(x => new { planId = x.PlanId, courseId = x.CourseId }))
                    {
                        string cid = pc.courseId;
                        string pid = pc.planId;

                        double planTotal = planScores.Sum(x => x.ScoreValues.FirstOrDefault(y => y.PlanId == pc.planId && y.CourseId == pc.courseId).Score);
                        ScoreTotalText += $"\n\t\t[{cid}] {pid}: {planTotal:F2}/{planScores.Sum(x => x.ScoreMax):F2} ({planTotal / planScores.Sum(x => x.ScoreMax) * 100.0:F2}%)";

                        PlanModel plan = Plans.FirstOrDefault(p => p.PlanId == pid);
                        if (plan != null)
                        {
                            plan.PlanScore = Math.Round(planTotal / planScores.Sum(x => x.ScoreMax) * 100.0 , 2);
                        }
                    }
                }
            }

            ScoreTotalText = ScoreTotalText;

        }
        
        // Event Methods
        private void OnPlanChanged(List<PlanModel> obj)
        {
            if (Plans != null)
            {
                Plans.Clear();
                foreach (var plan in obj.OrderByDescending(x => x.bPrimary))
                {
                    Plan = Patient.Courses.FirstOrDefault(x => x.Id == plan.CourseId).PlanSetups.FirstOrDefault(x => x.Id == plan.PlanId && x.Course.Id == plan.CourseId);
                    if (Plan == null)
                    {
                        Plan = Patient.Courses.FirstOrDefault(x => x.Id == plan.CourseId).PlanSums.FirstOrDefault(x => x.Id == plan.PlanId && x.Course.Id == plan.CourseId);
                    }
                    if (Plan != null)
                    {
                        Plans.Add(plan);
                    }
                }
                PlanScores.Clear();


                if (ScoreCard != null)
                {
                    ScorePlan(ScoreCard);
                }
            }
        }

        private void OnPluginVisible(bool obj)
        {
            bPluginVisibility = obj;
            if (obj)
            {
                PluginWidth = 300;

            }
            else
            {
                PluginWidth = 0;
            }
        }

    }
}
