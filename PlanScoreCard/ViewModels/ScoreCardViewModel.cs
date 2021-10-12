using PlanScoreCard.Events;
using PlanScoreCard.Events.Plugin;
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

namespace PlanScoreCard.ViewModels
{
    public class ScoreCardViewModel : BindableBase
    {

        private Application Application;
        private Patient Patient;
        private Course Course;
        private PlanningItem Plan;
        private IEventAggregator EventAggregator;


        private ScoreCardModel ScoreCard;
        private PatientSelectService PatientSelectService;


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


        ObservableCollection<PlanScoreModel> PlanScores { get; set; }
        ObservableCollection<PlanModel> Plans { get; set; }

        public ScoreCardViewModel(Application app, Patient patient, Course course, PlanSetup plan, IEventAggregator eventAggregator)
        {
            // Set the Initial Variables Passed In
            Application  = app;
            Patient = patient;
            Course = course;
            Plan = plan;
            EventAggregator = eventAggregator;

            // Need to change this event to take in a ScoreCardModel as the payload
            //EventAggregator.GetEvent<ScorePlanEvent>().Subscribe(OnScorePlan);
            EventAggregator.GetEvent<PluginVisibilityEvent>().Subscribe(OnPluginVisible);
            EventAggregator.GetEvent<PlanChangedEvent>().Subscribe(OnPlanChanged);
            
            
            PlanScores = new ObservableCollection<PlanScoreModel>();
            Plans = new ObservableCollection<PlanModel>();
            
            if (Plan != null)
            {
                OnPlanChanged(new List<PlanModel> { new PlanModel(Plan as PlanningItem, eventAggregator) { PlanId = Plan.Id, CourseId = Course.Id, bSelected = true } });
            }

            InitializeClass();
        }

        private void InitializeClass()
        {

            // Add the Plans
            // Clear the Collection of Plans
            Plans.Clear();

            // For each course, add all the Plans
            foreach (Course course in Patient.Courses)
            {
                foreach (PlanSetup plan in course.PlanSetups)
                    Plans.Add(new PlanModel(plan, EventAggregator));
            }
        }

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

        public void ScorePlan(ScoreCardModel scoreCard)
        {
            // _eventAggregator.GetEvent<UpdateTemplatesEvent>().Publish(_currentTemplate);
            PlanScores.Clear();

            // Get Collection of SelectedPlans
            List<PlanningItem> selectedPlans = Plans.Where(p => p.bSelected == true).Select(s => s._plan).ToList();
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
                    }
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
