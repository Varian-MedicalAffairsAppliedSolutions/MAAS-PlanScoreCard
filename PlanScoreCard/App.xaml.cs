using Autofac;
using PlanScoreCard.Startup;
using PlanScoreCard.ViewModels;
using PlanScoreCard.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

[assembly: ESAPIScript(IsWriteable = true)]
namespace PlanScoreCard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private VMS.TPS.Common.Model.API.Application _app;
        private string _patientId;
        private string _courseId;
        private string _planId;
        private Patient _patient;
        private Course _course;
        private PlanSetup _plan;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                var provider = new CultureInfo("en-US");
                DateTime endDate = DateTime.Now;
                if (DateTime.TryParse("12/31/2021", provider, DateTimeStyles.None, out endDate))
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
                                if (e.Args.Count() > 0 && !String.IsNullOrWhiteSpace(e.Args.First()))
                                {

                                    _patientId = e.Args.First().Split(';').First().Trim('\"');
                                }
                                else
                                {
                                    MessageBox.Show("Patient not specified at application start.");
                                    App.Current.Shutdown();
                                    return;

                                }
                                if (e.Args.First().Split(';').Count() > 1)
                                {
                                    _courseId = e.Args.First().Split(';').ElementAt(1).TrimEnd('\"');
                                }
                                if (e.Args.First().Split(';').Count() > 2)
                                {
                                    _planId = e.Args.First().Split(';').ElementAt(2).TrimEnd('\"');
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

                                var bootstrap = new Bootstrapper();
                                var container = bootstrap.Bootstrap(_patient, _course, _plan, _app.CurrentUser, _app);
                                ScoreCardView view = container.Resolve<ScoreCardView>();
                                view.ShowDialog();
                                _app.ClosePatient();
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
    }
}
