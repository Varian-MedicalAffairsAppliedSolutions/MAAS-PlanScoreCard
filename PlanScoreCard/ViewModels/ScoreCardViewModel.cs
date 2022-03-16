using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot.Wpf;
using PlanScoreCard.Events;
using PlanScoreCard.Events.Plugin;
using PlanScoreCard.Events.Plugins;
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
using System.Windows.Documents;
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
        private ProgressViewService ProgressViewService;
        private StructureDictionaryService StructureDictionaryService;

        private EditScoreCardView EditScoreCardView;

        // ScoreCard Object (Template)

        private ScoreCardModel scoreCard;
        public ScoreCardModel ScoreCard
        {
            get { return scoreCard; }
            set
            {
                SetProperty(ref scoreCard, value);
                ExportScoreCardCommand.RaiseCanExecuteChanged();
                PrintReportCommand.RaiseCanExecuteChanged();
            }
        }

        // ScoreTemplates - Legacy?
        private List<ScoreTemplateModel> ScoreTemplates;

        // Template Information
        private string TemplateName;
        private string TemplateSite;

        // Full Properties

        private string scoreTotalText;
        public string ScoreTotalText
        {
            get { return scoreTotalText; }
            set { SetProperty(ref scoreTotalText, value); }
        }

        // Patient ID
        private string patientId;

        public string PatientId
        {
            get { return patientId; }
            set { SetProperty(ref patientId, value); }
        }

        // Score Card Name 
        private string scoreCardName;

        public string ScoreCardName
        {
            get { return scoreCardName; }
            set { SetProperty(ref scoreCardName, value); }
        }

        // Max Score (Shown in Score Header)
        private double maxScore;

        public double MaxScore
        {
            get { return maxScore; }
            set { SetProperty(ref maxScore, value); }
        }

        // Selected Plan
        private PlanModel selectedPlan;

        public PlanModel SelectedPlan
        {
            get { return selectedPlan; }
            set
            {
                SetProperty(ref selectedPlan, value);
                ExportScoreCardCommand.RaiseCanExecuteChanged();
                PrintReportCommand.RaiseCanExecuteChanged();
            }
        }

        // Plan Scores
        private ObservableCollection<PlanScoreModel> planScores;
        public ObservableCollection<PlanScoreModel> PlanScores
        {
            get { return planScores; }
            set { SetProperty(ref planScores, value); }
        }

        // Plan Models
        private ObservableCollection<PlanModel> plans;
        public ObservableCollection<PlanModel> Plans
        {
            get { return plans; }
            set { SetProperty(ref plans, value); }
        }

        // Delegate Commands
        public DelegateCommand ScorePlanCommand { get; set; }
        public DelegateCommand ImportScoreCardCommand { get; set; }
        public DelegateCommand EditScoreCardCommand { get; set; }
        public DelegateCommand NormalizePlanCommand { get; set; }
        public DelegateCommand ExportScoreCardCommand { get; set; }
        public DelegateCommand PrintReportCommand { get; private set; }

        // Plugin Visibility

        private bool bpluginVisibility;
        public bool bPluginVisibility
        {
            get { return bpluginVisibility; }
            set { SetProperty(ref bpluginVisibility, value); }
        }

        // Plugin Width
        private int pluginWidth;
        public int PluginWidth
        {
            get { return pluginWidth; }
            set { SetProperty(ref pluginWidth, value); }
        }

        //warnings and flags 
        private bool _bWarning;

        public bool bWarning
        {
            get { return _bWarning; }
            set { SetProperty(ref _bWarning, value); }
        }
        private bool _bFlag;

        public bool bFlag
        {
            get { return _bFlag; }
            set { SetProperty(ref _bFlag, value); }
        }
        private string _warnings;

        public string Warnings
        {
            get { return _warnings; }
            set { SetProperty(ref _warnings, value); }
        }
        private string _flags;

        public string Flags
        {
            get { return _flags; }
            set { SetProperty(ref _flags, value); }
        }



        // Constructor
        public ScoreCardViewModel(Application app, Patient patient, Course course, PlanSetup plan, IEventAggregator eventAggregator, ViewLauncherService viewLauncherService, ProgressViewService progressViewService, StructureDictionaryService structureDictionaryService)
        {
            // Set the Initial Variables Passed In
            Application = app;
            Patient = patient;
            Course = course;
            Plan = plan;
            EventAggregator = eventAggregator;

            // Initiate Services
            ViewLauncherService = viewLauncherService;
            ProgressViewService = progressViewService;
            StructureDictionaryService = structureDictionaryService;

            // Need to change this event to take in a ScoreCardModel as the payload
            //EventAggregator.GetEvent<ScorePlanEvent>().Subscribe(OnScorePlan);
            EventAggregator.GetEvent<PluginVisibilityEvent>().Subscribe(OnPluginVisible);
            EventAggregator.GetEvent<PlanChangedEvent>().Subscribe(OnPlanChanged);
            EventAggregator.GetEvent<ScorePlanEvent>().Subscribe(ScorePlan);
            EventAggregator.GetEvent<PlanSelectedEvent>().Subscribe(ScorePlan);

            MaxScore = 0;

            // Initiate Collections
            PlanScores = new ObservableCollection<PlanScoreModel>();
            Plans = new ObservableCollection<PlanModel>();

            // Delegate Commands
            ScorePlanCommand = new DelegateCommand(ScorePlan);
            ImportScoreCardCommand = new DelegateCommand(ImportScoreCard);
            EditScoreCardCommand = new DelegateCommand(EditScoreCard);
            NormalizePlanCommand = new DelegateCommand(NormalizePlan);
            ExportScoreCardCommand = new DelegateCommand(ExportScoreCard, CanExportScorecard);
            PrintReportCommand = new DelegateCommand(OnPrintReport, CanPrintReport);

            // Sets If no Plan is Passed In
            if (Plan != null)
                OnPlanChanged(new List<PlanModel> { new PlanModel(Plan as PlanningItem, eventAggregator) { PlanId = Plan.Id, CourseId = Course.Id, bSelected = true } });

            InitializeClass();
        }

        private bool CanExportScorecard()
        {
            return ScoreCard != null && Plans.Any();
        }

        private bool CanPrintReport()
        {
            return ScoreCard != null && Plans.Any();
        }

        private void OnPrintReport()
        {
            var fd = new FlowDocument() { FontSize = 12, FontFamily = new System.Windows.Media.FontFamily("Franklin Gothic") };
            fd.Blocks.Add(new Paragraph(new Run($"Scorecard: {ScoreCard.Name} - {ScoreCard.SiteGroup}")) { TextAlignment = System.Windows.TextAlignment.Center });
            fd.Blocks.Add(new Paragraph(new Run($"Summary: {ScoreTotalText}")));
            foreach (var score in PlanScores)
            {
                if (!score.bPrintComment)
                {
                    score.bShowPrintComment = false;
                }
                var grid = new System.Windows.Controls.Grid();
                var col1 = new System.Windows.Controls.ColumnDefinition();
                col1.Width = new System.Windows.GridLength(16.5, System.Windows.GridUnitType.Star);
                var col2 = new System.Windows.Controls.ColumnDefinition();
                col2.Width = new System.Windows.GridLength(5, System.Windows.GridUnitType.Star);
                var scv = new ScoreReportView { DataContext = score };
                grid.ColumnDefinitions.Add(col1);
                grid.ColumnDefinitions.Add(col2);
                System.Windows.Controls.Grid.SetColumn(scv, 0);
                var innerGrid = new System.Windows.Controls.Grid();
                var row1 = new System.Windows.Controls.RowDefinition();
                row1.Height = new System.Windows.GridLength(3, System.Windows.GridUnitType.Star);
                var row2 = new System.Windows.Controls.RowDefinition();
                row2.Height = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star);
                var plotter = new System.Windows.Controls.Image()
                {
                    Source = new PngExporter() 
                    { 
                        Background = OxyPlot.OxyColors.LightGray}.ExportToBitmap(score.ScorePlotModel),
                        Height = 55, 
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,

                };
                System.Windows.Controls.Grid.SetRow(plotter, 0);
                var plotView = new ScorePlotView { DataContext = score };
                System.Windows.Controls.Grid.SetRow(plotView, 0);
                System.Windows.Controls.Grid.SetRowSpan(plotView, 2);
                innerGrid.RowDefinitions.Add(row1);
                innerGrid.RowDefinitions.Add(row2);
                System.Windows.Controls.Grid.SetColumn(innerGrid, 1);
                innerGrid.Children.Add(plotter);
                innerGrid.Children.Add(plotView);
                grid.Children.Add(scv);
                grid.Children.Add(innerGrid);
                fd.Blocks.Add(new BlockUIContainer(grid));
            }
            System.Windows.Controls.PrintDialog printer = new System.Windows.Controls.PrintDialog();
            fd.PageHeight = 1056;
            fd.PageWidth = 816;
            fd.PagePadding = new System.Windows.Thickness(50);
            fd.ColumnGap = 0;
            fd.ColumnWidth = 816;
            IDocumentPaginatorSource source = fd;
            if (printer.ShowDialog() == true)
            {
                printer.PrintDocument(source.DocumentPaginator, "Plan Scores");
            }
            foreach(var score in PlanScores)
            {
                score.bShowPrintComment = true;
            }
        }

        // Button Click Commands
        private void ExportScoreCard()
        {
            //implement this method when you get back (export scorecard values to  CSV).
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV File (.csv)|*.csv";
            sfd.Title = "Export Template to CSV";
            if (sfd.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(sfd.FileName))
                {
                    sw.WriteLine("Metric Id,Course Id,Plan Id,StructureId,Metric Text,Metric Value,Score,Max Score");
                    foreach (var template in PlanScores)
                    {
                        foreach (var point in template.ScoreValues)
                        {
                            sw.WriteLine($"{template.MetricId},{point.CourseId},{point.PlanId},{template.StructureId},{template.MetricText},{point.Value},{point.Score},{template.ScoreMax}");
                        }
                    }
                    sw.Flush();
                }
            }
            System.Windows.MessageBox.Show("Export Successful");
        }

        private void NormalizePlan()
        {
            if (Plans.Any(x => x.bPrimary) && ScoreTemplates.Count() > 0)
            {
                PluginViewService pluginViewService = new PluginViewService(EventAggregator);
                PluginViewModel pluginViewModel = new PluginViewModel(EventAggregator, pluginViewService);

                EventAggregator.GetEvent<ShowPluginViewEvent>().Publish();

                NormalizationService normService = new NormalizationService(Application, Patient, Plans.FirstOrDefault(x => x.bPrimary), ScoreTemplates, EventAggregator, StructureDictionaryService);

                var newplan = normService.GetPlan();
                Plans.Add(newplan);
                Plans.FirstOrDefault(x => x.CourseId == newplan.CourseId && x.PlanId == newplan.PlanId).bSelected = true;
            }
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
            if (Plan != null)
            {
                if (Plans.Any(x => x.PlanId == Plan.Id && x.CourseId == Course.Id))
                {
                    Plans.FirstOrDefault(x => x.PlanId == Plan.Id && x.CourseId == Course.Id).bPrimary = true;
                }
            }

            //Plans.First().bPrimary = true;

        }

        // Score Plan
        private void ScorePlan()
        {
            if (ScoreCard != null)
                ScorePlan(ScoreCard);

            ProgressViewService.Close();
        }

        public void ScorePlan(ScoreCardModel scoreCard)
        {
            if (EditScoreCardView != null)
            {
                if (EditScoreCardView.IsVisible)
                { EditScoreCardView.Hide(); }
            }

            ScoreCard = scoreCard;

            ProgressViewService.ShowProgress("Scoring Plans", 100, true);
            ProgressViewService.SendToFront();

            // _eventAggregator.GetEvent<UpdateTemplatesEvent>().Publish(_currentTemplate);
            PlanScores.Clear();

            // Get Collection of SelectedPlans
            List<PlanningItem> selectedPlans = Plans.Where(p => p.bSelected == true).Select(s => s.Plan as PlanningItem).ToList();

            // Convert the List to an Observable Collection
            ObservableCollection<PlanningItem> selectedPlanCollection = new ObservableCollection<PlanningItem>();
            foreach (PlanningItem plan in selectedPlans)
                selectedPlanCollection.Add(plan);

            // Initiate the MetricId Counter
            int metric_id = 0;

            // Loop through each Metric (ScoreTemplateModel)
            Warnings = String.Empty;
            Flags = String.Empty;
            foreach (ScoreTemplateModel template in scoreCard.ScoreMetrics)
            {
                // PlanScoreModel
                PlanScoreModel psm = new PlanScoreModel(Application, StructureDictionaryService);
                psm.BuildPlanScoreFromTemplate(selectedPlanCollection, template, metric_id);
                PlanScores.Add(psm);
                if (template.ScorePoints.Any(x => x.Variation) && psm.ScoreValues.Any(x => x.Score < template.ScorePoints.FirstOrDefault(y => y.Variation).Score))
                {
                    bWarning = true;
                    foreach (var scoreBelowVariation in psm.ScoreValues.Where(x => x.Score < template.ScorePoints.FirstOrDefault(y => y.Variation).Score))
                    {
                        Warnings += $"Course [{scoreBelowVariation.CourseId}]. Plan [{scoreBelowVariation.PlanId}. Metric {psm.MetricId} -- {psm.MetricText} below variation\n";
                    }
                }
                if (psm.ScoreValues.Any(x => x.Score == 0))
                {
                    bFlag = true;
                    foreach (var zeroScore in psm.ScoreValues.Where(x => x.Score == 0))
                    {
                        Flags += $"Course [{zeroScore.CourseId}]. Plan [{zeroScore.PlanId}. Metric {psm.MetricId + 1} -- {psm.MetricText}. Score is 0.\n";
                    }
                }
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

                        PlanModel plan = Plans.FirstOrDefault(p => p.PlanId == pid && p.CourseId == cid);

                        if (plan.PlanScore == null)
                            plan.PlanScore = 0;

                        if (plan != null)
                        {
                            plan.PlanScore = planTotal;
                        }

                        MaxScore = planScores.Sum(x => x.ScoreMax);
                        plan.MaxScore = MaxScore;
                    }
                }
                //if(PlanScores.Select(x=>x.ScoreValues.Any(y=>y.Score<y.)))
            }
            // not sure what the line below was supposed to accomplish. 
            //ScoreTotalText = ScoreTotalText;
            // Will delay for 3 seconds
            System.Threading.Thread.Sleep(700);
            ProgressViewService.Close();
        }

        private void EditScoreCard()
        {
            // Show the Progress Bar
            ProgressViewService.ShowProgress("Loading Scorecard", 100, true);

            ScoreCardModel scoreCard = new ScoreCardModel(TemplateName, TemplateSite, ScoreCard?.ScoreMetrics);
            EditScoreCardView = ViewLauncherService.GetEditScoreCardView();

            // Events
            EventAggregator.GetEvent<EditScoreCardSetPlanEvent>().Publish(new PlanModel(Plan, EventAggregator)); // Push the SelectedPlan
            EventAggregator.GetEvent<LoadEditScoreCardViewEvent>().Publish(scoreCard); // Push the ScoreCardModel to the ViewModel
            EventAggregator.GetEvent<EditScoreCardSetUserEvent>().Publish(Application.CurrentUser); // Push the User

            // Close the Progress Bar
            ProgressViewService.Close();

            //Show the View
            //EditScoreCardView.ShowDialog();
            EditScoreCardView.Visibility = System.Windows.Visibility.Visible;
        }

        // Load ScoreCard (Calls ScorePlan)
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
                        //this is done in BuildPlanScoreFromTemplate when parsing structure the templateId is set here. 
                        //else
                        //{
                        //    foreach (var scoreTemplate in template.ScoreTemplates)
                        //    {
                        //        //set template structure if it isn't set already. 
                        //        if (scoreTemplate.Structure.TemplateStructureId == null)
                        //        {
                        //            scoreTemplate.Structure.TemplateStructureId = scoreTemplate.Structure.StructureId;
                        //        }
                        //    }
                        //}
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


                    ScoreCard = new ScoreCardModel(TemplateName, TemplateSite, ScoreTemplates);

                }
                if (importSuccess)
                {
                    EventAggregator.GetEvent<ScorePlanEvent>().Publish(ScoreCard);
                }
            }
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
                EventAggregator.GetEvent<PlanToPluginEvent>().Publish(Plans.ToList());
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
        public void UpdatePlanModel(Patient patient, Course course, PlanSetup plan)
        {
            Patient = patient;
            Course = course;
            Plan = plan;
            //Plans.Clear();
            InitializeClass();
            //OnPlanChanged(new List<PlanModel> { new PlanModel(Plan as PlanningItem, EventAggregator) { PlanId = Plan.Id, CourseId = Course.Id, bSelected = true } });
        }
    }
}
