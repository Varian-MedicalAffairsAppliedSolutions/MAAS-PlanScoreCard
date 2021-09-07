using PlanScoreCard.Events.Plugin;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Services
{
    public class NormalizationService
    {
        public ObservableCollection<PlanScoreModel> PlanScores { get; private set; }

        private PlanModel _planModel;
        private String _patientId;
        private List<ScoreTemplateModel> _templates;
        private IEventAggregator _eventAggregator;
        private Application _app;
        private Patient _patient;

        public NormalizationService(Application app,Patient patient, PlanModel plan, 
            List<ScoreTemplateModel> templates, IEventAggregator eventAggregator)
        {
            _patient = patient;
            _templates = templates;
            _eventAggregator = eventAggregator;
            _app = app;
            //_patient = patient;
            PlanScores = new ObservableCollection<PlanScoreModel>();
            _planModel = plan;
            //GetPlan(plan);
            //_app.SaveModifications();
        }
        public PlanModel GetPlan()
        {
            //_app = VMS.TPS.Common.Model.API.Application.CreateApplication();
            //_patient = _app.OpenPatientById(_patientId);

            var course = _patient.Courses.FirstOrDefault(x => x.Id == _planModel.CourseId);
            var plan = course.PlanSetups.FirstOrDefault(x => x.Id == _planModel.PlanId);

            _eventAggregator.GetEvent<PlotUpdateEvent>().Publish("Series_X_Plan Normalization [%]");
            _eventAggregator.GetEvent<PlotUpdateEvent>().Publish("Series_Y_Score");
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Publish($"Accessing plan {plan.Id}");
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Publish($"\tInitial Normalization = {plan.PlanNormalizationValue}");

            //copy plan.
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Publish("Copying plan");
            _patient.BeginModifications();
            Course _newCourse = null;
            if (_patient.Courses.Any(x => x.Id == "N-Opt"))
            {
                _newCourse = _patient.Courses.FirstOrDefault(x => x.Id == "N-Opt");
            }
            else
            {
                _newCourse = _patient.AddCourse();
                _newCourse.Id = "N-Opt";
            }
            var _newPlan = _newCourse.CopyPlanSetup(plan);
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Publish($"Generated New Plan {_newPlan.Id} in {_newCourse.Id}");
            TestNormalization(_newPlan);
            var newPlanModel = new PlanModel(_newPlan, _eventAggregator)
            {
                CourseId = _newPlan.Course.Id,
                PlanId = _newPlan.Id
            };
            _app.SaveModifications();
            //_app.ClosePatient();
            //_app.Dispose();
            return newPlanModel;
        }

        private void TestNormalization(PlanSetup newPlan)
        {
            List<Tuple<double, double>> planScores = new List<Tuple<double, double>>();
            double initial_norm = newPlan.PlanNormalizationValue;
            for (double i = -20; i < 20; i += 2)
            {
                 ScorePlanAtNormValue(newPlan, planScores, initial_norm, i);
            }
            var maxScore = planScores.Max(x => x.Item2);
            var maxNorm = planScores.FirstOrDefault(x => x.Item2 == maxScore).Item1;
            for (double i = -2; i < 2; i += 0.2)
            {
                 ScorePlanAtNormValue(newPlan, planScores, maxNorm, i);
            }
            maxScore = planScores.Max(x => x.Item2);
            maxNorm = planScores.FirstOrDefault(x => x.Item2 == maxScore).Item1;
            for (double i = -0.2; i < 0.2; i += 0.01)
            {
                 ScorePlanAtNormValue(newPlan, planScores, maxNorm, i);
            }
            maxScore = planScores.Max(x => x.Item2);
            maxNorm = planScores.FirstOrDefault(x => x.Item2 == maxScore).Item1;
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Publish($"\tMax Score {maxScore:F3} changing normalization to {maxNorm}");
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Publish($"Activate Plan: {newPlan.Course};{newPlan}");
            newPlan.PlanNormalizationValue = maxNorm;
        }

        private void ScorePlanAtNormValue(PlanSetup newPlan, List<Tuple<double, double>> planScores, double initial_norm, double i)
        {
            double planNorm = initial_norm + i;
            newPlan.PlanNormalizationValue = planNorm;
            //var planScore = PlanScore.API.ScoreCardReader.ScorePlanFromTemplate(filename, new List<PlanningItem> { newPlan as PlanningItem });
            int metricId = 0;
            planScores.Clear();
            foreach (var template in _templates)
            {
                var psm = new PlanScoreModel(_app);
                psm.BuildPlanScoreFromTemplate(new ObservableCollection<PlanningItem> { newPlan }, template, metricId);
                metricId++;
                PlanScores.Add(psm);
            }
            var score = PlanScores.Sum(x => x.ScoreValues.First().Score);//.ScoreValues.Sum(x => x.Score);
            foreach (var metricScore in PlanScores)
            {
                if (metricScore.ScoreValues.Count() != 0)
                {
                    _eventAggregator.GetEvent<PlotUpdateEvent>().Publish($"Metric:<{metricScore.ScoreValues.FirstOrDefault().PlanId};{metricScore.MetricId};{metricScore.ScoreValues.FirstOrDefault().Value};{metricScore.ScoreValues.FirstOrDefault().Score}>");
                }
            }
            planScores.Add(new Tuple<double, double>(planNorm, score));
            _eventAggregator.GetEvent<ConsoleUpdateEvent>().Publish($"\t\tScore at {planNorm} = {score}");
            _eventAggregator.GetEvent<PlotUpdateEvent>().Publish($"PlotPoint:<{newPlan.Id};{planNorm};{score}>");
        }
    }
}
