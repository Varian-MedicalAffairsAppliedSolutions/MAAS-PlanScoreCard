using Autofac;
using PlanScoreCard.Events;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models;
using PlanScoreCard.Startup;
using PlanScoreCard.ViewModels;
using PlanScoreCard.ViewModels.VMHelpers;
using PlanScoreCard.Views;
using PlanScoreCard.Views.HelperWindows;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        //private VMS.TPS.Common.Model.API.Application _app;

        public void Application_Startup(object sender, StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            try
            {
                //this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                IEventAggregator eventAggregator = new EventAggregator();
                var startup = new StartupCore();
                startup.StartupApp(sender, e, eventAggregator);
            }
            catch (Exception ex)
            {
                //throw new ApplicationException(ex.Message);
                if (ConfigurationManager.AppSettings["Debug"] == "true")
                {
                    MessageBox.Show(ex.ToString());
                }
                //_app.ClosePatient();
                //_app.Dispose();
                App.Current.Shutdown();
            }
        }
    }
    public class StartupCore
    {
        private string _patientId;
        private string _courseId;
        private string _planId;
        public Patient _patient;
        public Course _course;
        public PlanSetup _plan;
        public ScoreCardView view;
        public VMS.TPS.Common.Model.API.Application _app;
        private EULAView eulaView;
        public List<PlanModel> plans;
        private IEventAggregator _eventAggregator;

        public Configuration GetUpdatedConfigFile()
        {
            var exePath = Assembly.GetExecutingAssembly().Location;
            var configPath = exePath + ".config";
            using (var fileStream = new FileStream(configPath, FileMode.Open))
            {
                if (!fileStream.CanWrite)
                {
                    System.Windows.MessageBox.Show($"Cannot update config file. \nUser does not have rights to {configPath}");
                    return null;
                }
            }
            //this needs to be the path running the application
            return ConfigurationManager.OpenExeConfiguration(exePath);
        }
        public void StartupApp(object sender, StartupEventArgs e, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            try
            {

                string argsString = e.Args.First();
                //var value = ConfigurationManager.AppSettings["EULAAgree"];
                //configFile.AppSettings.Settings.Remove("EULAAgree");
                //configFile.AppSettings.Settings["EULAAgree"].Value = "true";
                var configFile = GetUpdatedConfigFile();
                if (configFile != null && configFile.AppSettings.Settings["EulaAgree"].Value != "true")
                {
                    eventAggregator.GetEvent<CloseEulaEvent>().Subscribe(OnCloseEula);
                    eulaView = new EULAView();
                    eulaView.DataContext = new EULAViewModel(eventAggregator);
                    eulaView.ShowDialog();
                }
                var provider = new CultureInfo("en-US");
                DateTime endDate = DateTime.Now;
                var configUpdate = GetUpdatedConfigFile();
                var eulaValue = configUpdate.AppSettings.Settings["EulaAgree"].Value;
                var asmCa = typeof(StartupCore).Assembly.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(AssemblyExpirationDate));
                if (configUpdate != null && DateTime.TryParse(asmCa.ConstructorArguments.FirstOrDefault().Value as string, provider, DateTimeStyles.None, out endDate) && eulaValue == "true")
                {
                    if (DateTime.Now <= endDate)
                    {
                        string msg = $"The current planscorecard application is provided AS IS as a non-clinical, research only tool in evaluation only. The current " +
                            $"application will only be available until {endDate.Date} after which the application will be unavailable." +
                            $"By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support";
                        if (MessageBox.Show(msg, "Agreement  ", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            using (_app = VMS.TPS.Common.Model.API.Application.CreateApplication())
                            {
                                if (!String.IsNullOrWhiteSpace(argsString))
                                {

                                    _patientId = argsString.Split(';').First().Trim('\"');
                                }
                                else
                                {
                                    MessageBox.Show("Patient not specified at application start.");
                                    App.Current.Shutdown();
                                    return;

                                }
                                if (argsString.Split(';').Count() > 1)
                                {
                                    _courseId = argsString.Split(';').ElementAt(1).TrimEnd('\"');
                                }
                                if (argsString.Split(';').Count() > 2)
                                {
                                    _planId = argsString.Split(';').ElementAt(2).TrimEnd('\"');
                                }
                                if (String.IsNullOrWhiteSpace(_patientId) || String.IsNullOrWhiteSpace(_courseId))
                                {
                                    MessageBox.Show("Patient and/or Course not specified at application start. Please open a patient and course.");
                                    App.Current.Shutdown();
                                    return;
                                }
                                _patient = _app.OpenPatientById(_patientId);



                                if (!String.IsNullOrWhiteSpace(_courseId))
                                {
                                    _course = _patient.Courses.FirstOrDefault(x => x.Id == _courseId);
                                }
                                if (!String.IsNullOrEmpty(_planId))
                                {
                                    _plan = _course.PlanSetups.FirstOrDefault(x => x.Id == _planId);
                                }
                                //construct a planmodel and send that to the bootstrap instead.
                                plans = new List<PlanModel>();
                                foreach (var course in _patient.Courses)
                                {
                                    foreach (var plan in course.PlanSetups)
                                    {
                                        var localPlan = new PlanModel(plan, eventAggregator);
                                        if (plan.Id == _plan.Id && course.Id == _course.Id)
                                        {
                                            localPlan.bPrimary = true;
                                        }
                                        plans.Add(localPlan);
                                    }
                                    foreach (var planSum in course.PlanSums)
                                    {
                                        if (planSum.PlanSetups.Any())
                                        {
                                            plans.Add(new PlanModel(planSum, eventAggregator));
                                        }
                                    }
                                }
                                var bootstrap = new Bootstrapper();
                                var container = bootstrap.Bootstrap(plans, _app.CurrentUser, _app, eventAggregator);
                                view = container.Resolve<ScoreCardView>();
                                eventAggregator.GetEvent<UILaunchedEvent>().Publish();
                                view.ShowDialog();
                                _app.ClosePatient();
                                System.Windows.Application.Current.Shutdown();
                            }
                        }
                        else
                        {
                            System.Windows.Application.Current.Shutdown();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Application expiration date has been surpassed.");
                    }
                }
            }
            catch (Exception ex)
            {
                //throw new ApplicationException(ex.Message);
                if (ConfigurationManager.AppSettings["Debug"] == "true")
                {
                    MessageBox.Show(ex.ToString());
                }
                //_app.ClosePatient();
                _app.Dispose();
                App.Current.Shutdown();
            }

        }

        private void OnCloseEula()
        {
            if (eulaView != null)
            {
                eulaView.Close();
            }
        }

        public void ResetApplication(string feedback)
        {
            //NavigationViewModel.DisposePlugin();
            string _patientIdKeep = String.Empty;
            Patient _patient = null;
            List<PlanModel> plans = new List<PlanModel>();
            foreach (var line in feedback.Split('\n').Where(l => l.Contains("SelectedPlan:")))
            {
                string patientId = line.Split(':').Last().Split(';').First();
                //if (!patientId.Equals(_patientIdKeep))
                {
                    _app.ClosePatient();
                    _patient = _app.OpenPatientById(patientId);
                    _patientIdKeep = patientId;
                }
                if (_patient != null & line.Count(l => l == ';') > 1)
                {
                    bool bCourseExists = _patient.Courses.Any(c => c.Id.Equals(line.Split(';').ElementAt(1)));
                    if (bCourseExists)
                    {
                        Course course = _patient.Courses.First(c => c.Id.Equals(line.Split(';').ElementAt(1)));
                        bool bPlanExists = course.PlanSetups.Any(ps => ps.Id.Equals(line.Split(';').Last().TrimEnd('\r')));
                        if (bPlanExists)
                        {
                            PlanningItem plan = course.PlanSetups.First(ps => ps.Id.Equals(line.Split(';').Last().TrimEnd('\r')));
                            PlanModel planModel = new PlanModel(plan, _eventAggregator)
                            {
                                bSelected = true,
                            };
                            plans.Add(planModel);
                        }
                    }

                }

                //var bnewCourse = feedback.Split('\n').Last().Contains(';') ? _patient.Courses.Any(x => x.Id == feedback.Split('\n').Last().Split(';').First()) : false;
                //var _course = bnewCourse ?
                //    _patient.Courses.FirstOrDefault(x => x.Id == feedback.Split('\n').Last().Split(';').First())
                //    : _patient.Courses.FirstOrDefault(x => x.Id == _courseId);
                //var bnewPlan = feedback.Split('\n').Last().Contains(';') ? _course.PlanSetups.Any(x => x.Id == feedback.Split('\n').Last().Split(';').Last()) : false;
                //var _plan = bnewPlan ?
                //    _course.PlanSetups.FirstOrDefault(x => x.Id == feedback.Split('\n').Last().Split(';').Last())
                //    : _course.PlanSetups.FirstOrDefault(x => x.Id == _planId);
            }
                (view.DataContext as ScoreCardViewModel).UpdatePlanModel(plans);

            //ScoreCardViewModel.UpdatePlanModel(_patient, _course, _plan);
            //NavigationViewModel.UpdatePlanParameters(_patient, _course, _plan, feedbacks);
            //_eventAggregator.GetEvent<PlanChangedEvent>().Publish(NavigationViewModel.SelectedPlans.ToList());
        }
    }
}
