using Newtonsoft.Json;
//using NormalizePlan_Plugin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

[assembly: ESAPIScript(IsWriteable = true)]
namespace NormalizePlan_Plugin
{
    class Program
    {
        private static Patient _patient;
        private static string filename;

        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Plan Optimization through Normalization Application");
            Console.WriteLine("Series_X_Plan Normalization [%]");
            Console.WriteLine("Series_Y_Score");
            if (args.Count() > 1)
            {
                List<PlanModel> Plans = JsonConvert.DeserializeObject<List<PlanModel>>(File.ReadAllText(args.ElementAt(0)));
                filename = args.ElementAt(1);
                try
                {
                    using (Application app = VMS.TPS.Common.Model.API.Application.CreateApplication())
                    {
                        _patient = app.OpenPatientById(Plans.First().PatientId);
                        if (_patient != null)
                        {
                            Console.WriteLine($"Opening patient {_patient.Name}");
                            foreach (var plan in Plans)
                            {
                                if (_patient.Courses.FirstOrDefault(x => x.Id == plan.CourseId)?.PlanSetups?.FirstOrDefault(x => x.Id == plan.PlanId) != null)
                                {
                                    GetPlan(plan);
                                }
                            }
                        }

                        app.SaveModifications();
                        Console.WriteLine($"Plugin Operations Complete.\nYou may now click Reload Application.");
                        app.ClosePatient();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unhandled Exception. Unable to access Application");
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Not enough input arguments provided");
            }
        }

        private static void GetPlan(PlanModel planModel)
        {
            var course = _patient.Courses.FirstOrDefault(x => x.Id == planModel.CourseId);
            var plan = course.PlanSetups.FirstOrDefault(x => x.Id == planModel.PlanId);
            Console.WriteLine($"Accessing plan {plan.Id}");
            Console.WriteLine($"\tInitial Normalization = {plan.PlanNormalizationValue}");
            //copy plan.
            Console.WriteLine("Copying plan");
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
            Console.WriteLine($"Generated New Plan {_newPlan.Id} in {_newCourse.Id}");
            TestNormalization(_newPlan);
        }

        private static void TestNormalization(PlanSetup newPlan)
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
            Console.WriteLine($"\tMax Score {maxScore:F3} changing normalization to {maxNorm}");
            Console.WriteLine($"Activate Plan: {newPlan.Course};{newPlan}");
            newPlan.PlanNormalizationValue = maxNorm;
        }

        private static void ScorePlanAtNormValue(PlanSetup newPlan, List<Tuple<double, double>> planScores, double initial_norm, double i)
        {
            double planNorm = initial_norm + i;
            newPlan.PlanNormalizationValue = planNorm;
            var planScore = PlanScore.API.ScoreCardReader.ScorePlanFromTemplate(filename, new List<PlanningItem> { newPlan as PlanningItem });
            var score = planScore.Sum(x => x.ScoreValues.First().Score);//.ScoreValues.Sum(x => x.Score);
            foreach (var metricScore in planScore)
            {
                if (metricScore.ScoreValues.Count() != 0)
                {
                    Console.WriteLine($"Metric:<{metricScore.ScoreValues.FirstOrDefault().PlanId};{metricScore.MetricId};{metricScore.ScoreValues.FirstOrDefault().Value};{metricScore.ScoreValues.FirstOrDefault().Score}>");
                }
            }
            planScores.Add(new Tuple<double, double>(planNorm, score));
            Console.WriteLine($"\t\tScore at {planNorm} = {score}");
            Console.WriteLine($"PlotPoint:<{newPlan.Id};{planNorm};{score}>");
        }
        public class PlanModel
        {
            public string PatientId { get; set; }
            public string CourseId { get; set; }
            public string PlanId { get; set; }
        }
    }
}
