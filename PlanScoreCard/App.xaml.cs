using Autofac;
using PlanScoreCard.Events;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models;
using PlanScoreCard.Services;
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
using System.Windows.Media.Imaging;
using MAAS.Common.EulaVerification;

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
        // Helper method to check if a feature is enabled from EulaConfig, falling back to app.config only for compatibility
        private bool IsFeatureEnabled(EulaConfig eulaConfig, Configuration appConfig, string featureName, bool defaultValue = false)
        {
            // First check if the setting exists in EulaConfig
            if (eulaConfig != null && eulaConfig.Settings != null)
            {
                // Check for known properties
                if (featureName.Equals("EulaAgree", StringComparison.OrdinalIgnoreCase))
                {
                    return eulaConfig.Settings.EULAAgreed;
                }
                else if (featureName.Equals("ValidForClinicalUse", StringComparison.OrdinalIgnoreCase) ||
                         featureName.Equals("Validated", StringComparison.OrdinalIgnoreCase))
                {
                    return eulaConfig.Settings.Validated;
                }
            }

            // Fall back to app.config only for backward compatibility
            if (appConfig != null && appConfig.AppSettings.Settings[featureName] != null)
            {
                return appConfig.AppSettings.Settings[featureName].Value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            return defaultValue;
        }

        // Helper method to check if debug is enabled
        private bool IsDebugEnabled(EulaConfig eulaConfig, Configuration appConfig)
        {
            // Check from both sources, give precedence to EulaConfig
            bool debugEnabled = false;

            // First check app.config for backward compatibility
            if (appConfig != null && appConfig.AppSettings.Settings["Debug"] != null)
            {
                debugEnabled = appConfig.AppSettings.Settings["Debug"].Value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            // EulaConfig overrides if a setting exists there
            if (eulaConfig?.Settings != null)
            {
                // You could add a Debug property to ApplicationSettings if desired
                // For now, we'll just use the app.config value
            }

            return debugEnabled;
        }

        // Fallback debug check if EulaConfig or app config aren't available
        private bool IsDebugEnabledFallback()
        {
            try
            {
                return ConfigurationManager.AppSettings["Debug"] == "true";
            }
            catch
            {
                return false;
            }
        }
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
            // Declare these variables at a higher scope so they're available in the catch block
            Configuration configFile = null;
            EulaConfig eulaConfig = null;
            try
            {
                // TEMP FOR DEBUG STARTUP
                string argsString = e.Args.First();

                //var value = ConfigurationManager.AppSettings["EULAAgree"];
                //configFile.AppSettings.Settings.Remove("EULAAgree");
                //configFile.AppSettings.Settings["EULAAgree"].Value = "true";
                configFile = GetUpdatedConfigFile();
                bool skipAgree = false;
                if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "NoAgree.txt")))
                {
                    skipAgree = true;
                }
                //if (configFile != null && configFile.AppSettings.Settings["EulaAgree"].Value != "true" && !skipAgree)
                // Define the project information for EULA verification
                const string PROJECT_NAME = "PlanScoreCard";
                const string PROJECT_VERSION = "1.0.0";
                const string LICENSE_URL = "https://varian-medicalaffairsappliedsolutions.github.io/MAAS-PlanScoreCard";
                const string GITHUB_URL = "https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard";

                // Get the application directory for config storage
                string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                System.Diagnostics.Debug.WriteLine($"Application directory: {appDirectory}");

                // Instead of trying to set ConfigDirectory directly, create the EulaConfig with the correct path
                string configFilePath = Path.Combine(appDirectory, $"{PROJECT_NAME}_EulaConfig.xml");
                System.Diagnostics.Debug.WriteLine($"Expected EulaConfig path: {configFilePath}");

                // Get access to the EulaConfig for diagnostic purposes, but let the EulaVerifier handle saving
                eulaConfig = EulaConfig.Load(PROJECT_NAME);
                if (eulaConfig == null || eulaConfig.Settings == null)
                {
                    // Only create a new EulaConfig if it doesn't exist
                    eulaConfig = new EulaConfig();
                    eulaConfig.Settings = new ApplicationSettings();
                }

                // Update EulaConfig with any existing settings from app.config for backward compatibility
                if (configFile != null && configFile.AppSettings.Settings["EulaAgree"] != null)
                {
                    bool oldEulaValue = configFile.AppSettings.Settings["EulaAgree"].Value == "true";
                    if (oldEulaValue && !eulaConfig.Settings.EULAAgreed)
                    {
                        eulaConfig.Settings.EULAAgreed = true;
                        eulaConfig.Save();
                    }
                }

                if (!skipAgree)
                {
                    //eventAggregator.GetEvent<CloseEulaEvent>().Subscribe(OnCloseEula);
                    //eulaView = new EULAView();
                    //eulaView.DataContext = new EULAViewModel(eventAggregator);
                    //eulaView.ShowDialog();
                    // Create the EulaVerifier - this will load/save config as needed
                    var eulaVerifier = new EulaVerifier(PROJECT_NAME, PROJECT_VERSION, LICENSE_URL);

                    // If the JotForm license hasn't been accepted for this version, show the verification dialog
                    if (!eulaVerifier.IsEulaAccepted())
                    {
                        // Diagnose before showing dialog
                        DiagnoseEulaVerification(PROJECT_NAME, PROJECT_VERSION, eulaConfig);

                        MessageBox.Show(
                            $"This version of {PROJECT_NAME} (v{PROJECT_VERSION}) requires license acceptance before first use.\n\n" +
                            "You will be prompted to provide an access code. Please follow the instructions to obtain your code.",
                            "License Acceptance Required",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // Load the QR code image
                        System.Windows.Media.Imaging.BitmapImage qrCode = null;
                        try
                        {
                            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                            //qrCode = new System.Windows.Media.Imaging.BitmapImage(new Uri($"pack://application:,,,/{assemblyName};component/Resources/qrcode.bmp"));
                            qrCode = new System.Windows.Media.Imaging.BitmapImage(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"Resources","qrcode.bmp")));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error loading QR code: {ex.Message}");
                        }

                        // Show EULA dialog - this will save the entry to the XML file if accepted
                        bool accepted = eulaVerifier.ShowEulaDialog(qrCode);
                        if (!accepted)
                        {
                            MessageBox.Show(
                                "License acceptance is required to use this application.\n\n" +
                                "The application will now close.",
                                "License Not Accepted",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            App.Current.Shutdown();
                            return;
                        }

                        // After EULA verification, reload the config to get the latest values
                        eulaConfig = EulaConfig.Load(PROJECT_NAME);

                        // Perform diagnostics to help troubleshoot
                        DiagnoseEulaVerification(PROJECT_NAME, PROJECT_VERSION, eulaConfig);
                    }
                    else
                    {
                        // EULA already accepted, still do a diagnostic for troubleshooting
                        DiagnoseEulaVerification(PROJECT_NAME, PROJECT_VERSION, eulaConfig);
                    }
                }
                var provider = new CultureInfo("en-US");
                DateTime endDate = DateTime.Now;
                //var configUpdate = GetUpdatedConfigFile();
                //var eulaValue = skipAgree?"true":configUpdate.AppSettings.Settings["EulaAgree"].Value;
                var asmCa = typeof(StartupCore).Assembly.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(AssemblyExpirationDate));
                var bNoExpire = File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "NOEXPIRE"));


                //if (configUpdate != null && DateTime.TryParse(asmCa.ConstructorArguments.FirstOrDefault().Value as string, provider, DateTimeStyles.None, out endDate) && eulaValue == "true")
                // Parse the expiration date from assembly attribute
                if (asmCa != null && DateTime.TryParse(asmCa.ConstructorArguments.FirstOrDefault().Value as string, provider, DateTimeStyles.None, out endDate))
                {
                    //Check to see if application has expired. 
                    if (DateTime.Now <= endDate || bNoExpire)
                    {
                        //string msg = $"The current planscorecard application is provided AS IS as a non-clinical, research only tool in evaluation only. The current " +
                        //    $"application will only be available until {endDate.Date} after which the application will be unavailable." +
                        //    $"By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support\n\n"+
                        //    "Newer builds with future expiration dates can be found here: https://github.com/Varian-MedicalAffairsAppliedSolutions/MAAS-PlanScoreCard\n\n" +
                        //    "See the FAQ for more information on how to remove this pop-up and expiration";
                        //bool userAgree = false;
                        //if (!skipAgree)
                        //{
                        //    userAgree = MessageBox.Show(msg, "Agreement  ", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
                        //}
                        //if (skipAgree || userAgree)
                        // Check for NoAgree.txt at assembly location
                        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        var noexp_path = Path.Combine(path, "NOEXPIRE");
                        bool foundNoExpire = File.Exists(noexp_path);

                        // Only show the agreement message if not using NoAgree and settings exist
                        if (!foundNoExpire && eulaConfig.Settings != null)
                        {
                            // Get validation status for display only - do not update it automatically
                            bool isValidated = IsFeatureEnabled(eulaConfig, configFile, "ValidForClinicalUse");

                            string msg = $"The current {PROJECT_NAME} application is provided AS IS as a non-clinical, research only tool in evaluation only. The current " +
                                $"application will only be available until {endDate.Date} after which the application will be unavailable. " +
                                "By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support\n\n" +
                                $"Newer builds with future expiration dates can be found here: {GITHUB_URL}\n\n" +
                                "See the FAQ for more information on how to remove this pop-up and expiration";

                            string msg2 = $"Application will only be available until {endDate.Date} after which the application will be unavailable. " +
                                "By Clicking 'Yes' you agree that this application will be evaluated and not utilized in providing planning decision support\n\n" +
                                $"Newer builds with future expiration dates can be found here: {GITHUB_URL} \n\n" +
                                "See the FAQ for more information on how to remove this pop-up and expiration";

                            if (!eulaConfig.Settings.Validated)
                            {
                                // Show the first-time message
                                var res = MessageBox.Show(msg, "Agreement  ", MessageBoxButton.YesNo);
                                if (res == MessageBoxResult.No)
                                {
                                    App.Current.Shutdown();
                                    return;
                                }

                                // Don't update validation settings, only the user should do this
                            }
                            else
                            {
                                // Show the returning user message
                                var res = MessageBox.Show(msg2, "Agreement  ", MessageBoxButton.YesNo);
                                if (res == MessageBoxResult.No)
                                {
                                    App.Current.Shutdown();
                                    return;
                                }
                            }
                        }
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
                            //now only loading first initial plan and then making a button to load all plans. 
                            plans = new List<PlanModel>();
                            if (_plan.StructureSet != null)
                            {
                                //plans.Add(new PlanModel(_plan,eventAggregator));
                                var localPlan = new PlanModel(_plan, eventAggregator);
                                localPlan.bPrimary = true;
                                plans.Add(localPlan);
                            }

                            var bootstrap = new Bootstrapper();
                            var container = bootstrap.Bootstrap(plans, _app.CurrentUser, _app, eventAggregator);
                            StructureDictionaryService.ReadStructureDictionary();
                            view = container.Resolve<ScoreCardView>();
                            eventAggregator.GetEvent<UILaunchedEvent>().Publish();
                            view.ShowDialog();
                            _app.ClosePatient();
                            System.Windows.Application.Current.Shutdown();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Application expiration date has been surpassed.");
                        App.Current.Shutdown();
                    }
                }
                else
                {
                    MessageBox.Show("Unable to launch application");
                    App.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                //throw new ApplicationException(ex.Message);
                bool showDebug = false;

                // Try multiple approaches to determine if debug is enabled
                try
                {
                    showDebug = IsDebugEnabled(eulaConfig, configFile) || IsDebugEnabledFallback();
                }
                catch
                {
                    // Last resort - direct check
                    try
                    {
                        showDebug = ConfigurationManager.AppSettings["Debug"] == "true";
                    }
                    catch
                    {
                        // If all else fails, don't show debug info
                    }
                }
                if (showDebug)
                {
                    MessageBox.Show(ex.ToString());
                }
                //_app.ClosePatient();
                try { _app?.Dispose(); } catch { }
                App.Current.Shutdown();
            }

        }
        // Helper method to diagnose EULA verification issues
        private void DiagnoseEulaVerification(string projectName, string version, EulaConfig eulaConfig)
        {
            try
            {
                // Get the expected config key
                string configKey = $"{projectName}-{version}";
                System.Diagnostics.Debug.WriteLine($"Checking for EULA entry with key: {configKey}");

                // Check if EulaConfig has any entries
                if (eulaConfig.EulaEntries == null || eulaConfig.EulaEntries.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("EulaConfig has no EulaEntries");
                }
                else
                {
                    // Log all entries
                    System.Diagnostics.Debug.WriteLine($"EulaConfig has {eulaConfig.EulaEntries.Count} entries:");
                    foreach (var entry in eulaConfig.EulaEntries)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Key: {entry.Key}, Value: {entry.Value}");
                    }

                    // Check for the specific entry we need
                    var targetEntry = eulaConfig.EulaEntries.FirstOrDefault(e => e.Key == configKey);
                    if (targetEntry != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found matching entry: Key={targetEntry.Key}, Value={targetEntry.Value}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No entry found with key: {configKey}");
                    }
                }

                // Check the settings
                if (eulaConfig.Settings != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Settings: EULAAgreed={eulaConfig.Settings.EULAAgreed}, Validated={eulaConfig.Settings.Validated}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Settings object is null");
                }

                // Check the config file path
                string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string expectedPath = Path.Combine(appDir, $"{projectName}_EulaConfig.xml");
                System.Diagnostics.Debug.WriteLine($"Expected config file path: {expectedPath}");
                bool fileExists = File.Exists(expectedPath);
                System.Diagnostics.Debug.WriteLine($"File exists: {fileExists}");

                // If the file exists, let's output its contents for debugging
                if (fileExists)
                {
                    try
                    {
                        string content = File.ReadAllText(expectedPath);
                        System.Diagnostics.Debug.WriteLine($"File content:\n{content}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error reading file: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DiagnoseEulaVerification: {ex.Message}");
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
