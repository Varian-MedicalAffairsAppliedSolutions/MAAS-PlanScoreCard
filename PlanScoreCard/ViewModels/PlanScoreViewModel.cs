using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
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
    public class PlanScoreViewModel : BindableBase
    {
        private string _scoreTotalText;

        public string ScoreTotalText
        {
            get { return _scoreTotalText; }
            set { SetProperty(ref _scoreTotalText, value); }
        }

        private string _patientId;
        private string _courseId;
        private string _planId;
        private PlanningItem _plan;
        private Course _course;
        private Application _app;
        private IEventAggregator _eventAggregator;
        private List<ScoreTemplateModel> _currentTemplate;
        private Patient _patient;

        public ObservableCollection<PlanScoreModel> PlanScores { get; set; }
        public ObservableCollection<PlanningItem> Plans { get; private set; }



        public PlanScoreViewModel(Application app, Patient patient, Course course, PlanSetup plan, IEventAggregator eventAggregator)
        {
            _patientId = patient.Id;
            _courseId = course.Id;
            _planId = plan.Id;
            _patient = patient;
            _plan = plan;
            _course = course;
            _app = app;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<PlanChangedEvent>().Subscribe(OnPlanChanged);
            PlanScores = new ObservableCollection<PlanScoreModel>();
            Plans = new ObservableCollection<PlanningItem>();
            if (_plan != null)
            {
                OnPlanChanged(new List<PlanModel> { new PlanModel(_plan as PlanningItem, _eventAggregator) { PlanId = _plan.Id, CourseId = _course.Id, bSelected = true } });
            }
           
        }


        public void UpdatePlanModel(Patient patient, Course course, PlanSetup plan)
        {
            _patient = patient;
            _course = course;
            _plan = plan;
        }
        private void OnPlanChanged(List<PlanModel> plans)
        {
            if (plans != null)
            {
                Plans.Clear();
                foreach (var plan in plans.OrderByDescending(x=>x.bPrimary))
                {
                    _plan = _patient.Courses.FirstOrDefault(x => x.Id == plan.CourseId).PlanSetups.FirstOrDefault(x => x.Id == plan.PlanId && x.Course.Id == plan.CourseId);
                    if(_plan == null)
                    {
                        _plan = _patient.Courses.FirstOrDefault(x => x.Id == plan.CourseId).PlanSums.FirstOrDefault(x => x.Id == plan.PlanId && x.Course.Id == plan.CourseId);
                    }
                    if (_plan != null)
                    {
                        Plans.Add(_plan);
                    }
                }
                PlanScores.Clear();
                if (_currentTemplate != null)
                {
                    ScorePlan(_currentTemplate);
                }
            }
        }

        public void ScorePlan(List<ScoreTemplateModel> templates)
        {
            _currentTemplate = templates;
            // _eventAggregator.GetEvent<UpdateTemplatesEvent>().Publish(_currentTemplate);
            PlanScores.Clear();
            int metric_id = 0;
            foreach (var template in templates)
            {
                var psm = new PlanScoreModel(_app);
                psm.BuildPlanScoreFromTemplate(Plans, template, metric_id);
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
    }
}
