using Microsoft.Win32;
using Newtonsoft.Json;
using OxyPlot.Wpf;
using PlanScoreCard.Events;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Events.Plugin;
using PlanScoreCard.Events.Plugins;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Models.Proknow;
using PlanScoreCard.Services;
using PlanScoreCard.ViewModels.VMHelpers;
using PlanScoreCard.Views;
using PlanScoreCard.Views.HelperWindows;
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
using System.Windows.Documents;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.ViewModels
{
    public class ScoreCardViewModel : BindableBase
    {

        // Class Variables 

        private Application Application;
        private string _patientId;                      
        private Patient _patient;
        private bool _isWindowActive;
        //private Course Course;            
        private PlanModel Plan;
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
                NormalizePlanCommand.RaiseCanExecuteChanged();
            }
        }
        //private bool _validated;

        //public bool Validated
        //{
        //    get { return _validated; }
        //    set { _validated = value; }
        //}
        private string _scoreCardTitle;

        public string ScoreCardTitle
        {
            get { return _scoreCardTitle; }
            set { SetProperty(ref _scoreCardTitle, value); }
        }
        // ScoreTemplates - Legacy?
        //Scorecard should be used going forward but ScoreTemplates is currently the variable that gets the score card metrics on import.
        //Then 'ScoreCard' gets the template name, creator, Rx and ScoreTemplates is the ScoreMetrics property of ScoreCard.
        //But ScoreCard gets updated on change so use that after. 
        private List<ScoreTemplateModel> ScoreTemplates;

        // Template Information
        private string TemplateName;
        private string TemplateSite;
        private double _dosePerFraction;

        public double DosePerFraction
        {
            get { return _dosePerFraction; }
            set
            {
                SetProperty(ref _dosePerFraction, value);
            }
        }
        //Number of fractions controls the events when there are changes (it get set last). 
        private int _numberOfFractions;

        public int NumberOfFractions
        {
            get { return _numberOfFractions; }
            set
            {
                SetProperty(ref _numberOfFractions, value);
                SetRxMessage();


            }
        }
        private bool bDoseMatch;
        private bool bFxMatch;
        private void SetRxMessage()
        {
            if (DosePerFraction == 0.0 || NumberOfFractions == 0)
            {
                RxMessage = "Scorecard Rx not set";
                //bRxScalingVisibility = false;
            }
            else
            {
                if (!Plan.bPlanSum)
                {
                    CheckPlanRx(out bDoseMatch, out bFxMatch);
                    if (!bDoseMatch || !bFxMatch)
                    {
                        RxMessage = "Plan Rx does not match scorecard.";
                        //bRxScalingVisibility = true;
                    }
                    else
                    {
                        RxMessage = "Plan Rx matches scorecard.";
                        //bRxScalingVisibility = false;
                    }
                    if (NumberOfFractions == 0 || DosePerFraction == 0.0)
                    {
                        bRxScalingVisibility = false;
                    }
                    else if (bDoseMatch && bFxMatch)
                    {
                        bRxScalingVisibility = false;
                    }
                    else
                    {
                        bRxScalingVisibility = true;
                    }
                }
                else
                {
                    RxMessage = "No Rx for PlanSum.";
                    bRxScalingVisibility = false;
                }
            }
        }

        private void CheckPlanRx(out bool bDoseMatch, out bool bFxMatch)
        {
            bool bGyUnit = true;
            if (Plan.DoseUnit != VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy)
            {
                bGyUnit = false;
            }
            bDoseMatch = bGyUnit ?
                Math.Abs(DosePerFraction - Plan.DosePerFraction) < 0.1 :
                Math.Abs(DosePerFraction - Plan.DosePerFraction / 100.0) < 0.1;
            bFxMatch = Math.Abs(NumberOfFractions - Plan.NumberOfFractions) < 1;
        }

        private string _rxMessage;

        public string RxMessage
        {
            get { return _rxMessage; }
            set
            {
                SetProperty(ref _rxMessage, value);
            }
        }
        private bool _bRxScaling;

        public bool bRxScaling
        {
            get { return _bRxScaling; }
            set
            {
                SetProperty(ref _bRxScaling, value);
                if (bRxScaling)
                {
                    if (!Plan.bPlanSum)
                    {
                        ScaleScorecard(DosePerFraction * NumberOfFractions, Plan.DosePerFraction * Plan.NumberOfFractions, Plan.DoseUnit.ToString());
                    }
                }
            }
        }
        private bool _bScaled;
        private void ScaleScorecard(double scoreCardTotalDose, double planTotalDose, string planDoseUnit)
        {
            if (ScoreCard == null)
            {
                return;
            }
            InvalidateScores();
            //Reminder plan.DosePerFraction is always in Gy
            //var planTotalDoseGy = planDoseUnit.StartsWith("c") ? planTotalDose  : planTotalDose;

            foreach (var template in ScoreCard.ScoreMetrics)
            {
                if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.DoseAtVolume ||
                    (MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.MaxDose ||
                    (MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.MinDose ||
                    (MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.MeanDose)
                {
                    if (!template.OutputUnit.Contains("%"))
                    {
                        foreach (var scorePoint in template.ScorePoints)
                        {
                            //conversions not needed because planTotalDose and scoreCardTotalDose are in the same unit (Gy)
                            scorePoint.PointX =  scorePoint.PointX * planTotalDose / scoreCardTotalDose;
                        }
                    }
                }
                else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.VolumeAtDose ||
                (MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.ConformationNumber ||
                (MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.ConformityIndex)
                {
                    if (!template.InputUnit.Contains("%"))
                    {
                        template.InputValue = template.InputValue * planTotalDose / scoreCardTotalDose;
                    }
                }
                else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.HomogeneityIndex)
                {
                    if (!template.HI_TargetUnit.Contains("%"))
                    {
                        template.HI_Target = template.HI_Target * planTotalDose / scoreCardTotalDose;
                    }
                }
            }
            _bScaled = true;
            ScoreCard.DosePerFraction = DosePerFraction = planTotalDose / Plan.NumberOfFractions;
            ScoreCard.NumberOfFractions = NumberOfFractions = Plan.NumberOfFractions;
            ScorePlan();
        }

        private bool _bRxScalingVisibility;

        public bool bRxScalingVisibility
        {
            get { return _bRxScalingVisibility; }
            set { SetProperty(ref _bRxScalingVisibility, value); }
        }


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
                NormalizePlanCommand.RaiseCanExecuteChanged();
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
        public DelegateCommand OpenWarningCommand { get; private set; }
        public DelegateCommand OpenFlagCommand { get; private set; }
        public DelegateCommand OpenPatientSelectionCommand { get; private set; }

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

        public MessageView MessageView { get; set; }

        // Constructor
        public ScoreCardViewModel(Application app, List<PlanModel> plans, IEventAggregator eventAggregator, ViewLauncherService viewLauncherService, ProgressViewService progressViewService, StructureDictionaryService structureDictionaryService)
        {
            // Set the Initial Variables Passed In
            ScoreCardTitle = ConfigurationManager.AppSettings["ValidForClinicalUse"] != "true" ?
                "Plan ScoreCard **Not Validated for Clinical Use**" : "Plan ScoreCard";
            Application = app;
            //Patient = patient;
            //Course = course;
            //Plan = plan;
            EventAggregator = eventAggregator;

            // Initiate Services
            ViewLauncherService = viewLauncherService;
            ProgressViewService = progressViewService;
            StructureDictionaryService = structureDictionaryService;

            // Need to change this event to take in a ScoreCardModel as the payload
            //EventAggregator.GetEvent<ScorePlanEvent>().Subscribe(OnScorePlan);
            EventAggregator.GetEvent<PluginVisibilityEvent>().Subscribe(OnPluginVisible);

            EventAggregator.GetEvent<ScorePlanEvent>().Subscribe(ScorePlan);
            EventAggregator.GetEvent<PlanSelectedEvent>().Subscribe(ScorePlan);
            EventAggregator.GetEvent<FreePrimarySelectionEvent>().Subscribe(OnPrimaryChanged);
            EventAggregator.GetEvent<CloseMessageViewEvent>().Subscribe(OnCloseMessage);
            EventAggregator.GetEvent<UpdatePatientPlansEvent>().Subscribe(OnUpdatePatientPlans);
            EventAggregator.GetEvent<ClosePatientSelectionEvent>().Subscribe(OnClosePatientSelection);
            //EventAggregator.GetEvent<RemovePlanFromScoreEvent>().Subscribe(OnRemovePlanFromScore);

            MaxScore = 0;

            // Initiate Collections
            PlanScores = new ObservableCollection<PlanScoreModel>();
            Plans = new ObservableCollection<PlanModel>();
            foreach (var planModel in plans)
            {
                Plans.Add(planModel);
            }
            // Delegate Commands
            ScorePlanCommand = new DelegateCommand(ScorePlan);
            ImportScoreCardCommand = new DelegateCommand(ImportScoreCard);
            EditScoreCardCommand = new DelegateCommand(EditScoreCard);
            NormalizePlanCommand = new DelegateCommand(NormalizePlan, CanNormalizePlan);
            ExportScoreCardCommand = new DelegateCommand(ExportScoreCard, CanExportScorecard);
            PrintReportCommand = new DelegateCommand(OnPrintReport, CanPrintReport);
            OpenWarningCommand = new DelegateCommand(OnOpenWarning);
            OpenFlagCommand = new DelegateCommand(OnOpenFlag);
            OpenPatientSelectionCommand = new DelegateCommand(OnOpenPatientSelector);
            bRxScalingVisibility = true;
            // Sets If no Plan is Passed In

            //if (Plan != null)
            //    OnPlanChanged(new List<PlanModel> { });
            if (plans.Any(p => p.bPrimary))
            {
                OnPlanChanged(Plans.ToList());
            }
            InitializeClass();
            //I moved this down here so that the scoreplan doesn't run until after the plans have already been setup (the event aggregator was running on every plan).
            EventAggregator.GetEvent<PlanChangedEvent>().Subscribe(OnPlanChanged);
        }

        private void OnRemovePlanFromScore(PlanModel obj)
        {
            throw new NotImplementedException();
        }

        private void OnUpdatePatientPlans(List<PatientPlanSearchModel> obj)
        {
            Plans.Clear();
            foreach (var planSearchModel in obj)
            {
                if (planSearchModel.Plans.Any(psm => psm.bSelected))
                {
                    foreach (var plan in planSearchModel.Plans.Where(pl => pl.bSelected))
                    {
                        Plans.Add(plan);
                    }
                }
            }
        }

        private void OnClosePatientSelection(bool isSaved)
        {
            _isWindowActive = true;
            if (patientSelectionView != null)
            {
                patientSelectionView.Close();
                patientSelectionView = null;
            }
            if (isSaved)
            {
                OnPlanChanged(Plans.ToList());
            }
        }

        public PatientSelectionView patientSelectionView;
        private void OnOpenPatientSelector()
        {
            _isWindowActive = false;
            patientSelectionView = new PatientSelectionView();
            patientSelectionView.DataContext = new PatientSelectionViewModel(EventAggregator, Application, Plans.ToList(), ScoreCard);
            
            patientSelectionView.ShowDialog();
        }

        private void OnCloseMessage()
        {
            MessageView.Close();
            MessageView = null;
        }

        private void OnOpenFlag()
        {
            if (MessageView == null)
            {
                MessageView = new MessageView();
            }
            else
            {
                MessageView = null;
                MessageView = new MessageView();
            }
            MessageView.DataContext = new MessageViewModel("Scorecard Flags", Flags, EventAggregator);
            MessageView.ShowDialog();
        }

        private void OnOpenWarning()
        {
            if (MessageView == null)
            {
                MessageView = new MessageView();
            }
            else
            {
                MessageView = null;
                MessageView = new MessageView();
            }
            MessageView.DataContext = new MessageViewModel("Scorecard Warnings", Warnings, EventAggregator);
            MessageView.ShowDialog();
        }

        private void OnPrimaryChanged(PlanModel obj)
        {
            if (obj.bPrimary)
            {
                foreach (var plan in Plans)
                {
                    if (plan != obj)
                    {
                        plan.bPrimary = false;
                    }
                }
                Plan = obj;//obj.bPlanSum ?
                           //Patient.Courses.FirstOrDefault(x => x.Id == obj.CourseId).PlanSums.FirstOrDefault(x => x.Id == obj.PlanId) as PlanningItem :
                           //Patient.Courses.FirstOrDefault(x => x.Id == obj.CourseId).PlanSetups.FirstOrDefault(x => x.Id == obj.PlanId) as PlanningItem;
                _bScaled = false;
                bRxScaling = false;
                SetRxMessage();
                //DosePerFraction = obj.DosePerFraction;
                //NumberOfFractions = obj.NumberOfFractions;
            }
            ScorePlan();
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
                List<double> thicknesses = new List<double>();
                //foreach(var series in score.ScorePlotModel.Series)
                //{
                //    thicknesses.Add((series as LineSeries).StrokeThickness);
                //}
                var plotter = new System.Windows.Controls.Image()
                {
                    //var thickness = (score.ScorePlotModel.Series.First() as LineSeries).StrokeThickness
                    Source = new PngExporter()
                    {

                        Background = OxyPlot.OxyColors.LightGray
                    }.ExportToBitmap(score.ScorePlotModel),
                    Height = 55,
                    Width = 300
                    //HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,

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
            foreach (var score in PlanScores)
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
                    sw.WriteLine("Metric Id,Patient Id,Course Id,Plan Id,StructureId,Metric Text,Metric Value,Score,Max Score");
                    foreach (var template in PlanScores)
                    {
                        foreach (var point in template.ScoreValues)
                        {
                            sw.WriteLine($"{template.MetricId},{point.PatientId},{point.CourseId},{point.PlanId},{template.StructureId},{template.MetricText},{point.Value},{point.Score},{template.ScoreMax}");
                        }
                    }
                    sw.Flush();
                }
            }
            System.Windows.MessageBox.Show("Export Successful");
        }
        private bool CanNormalizePlan()
        {
            return Plans.Any(x => x.bPrimary) && ScoreCard != null;
        }

        private void NormalizePlan()
        {
            if (Plans.Any(x => x.bPrimary) && ScoreCard.ScoreMetrics.Count() > 0)
            {
                PluginViewService pluginViewService = new PluginViewService(EventAggregator);
                PluginViewModel pluginViewModel = new PluginViewModel(EventAggregator, pluginViewService);

                EventAggregator.GetEvent<ShowPluginViewEvent>().Publish();

                NormalizationService normService = new NormalizationService(Application, Plans.FirstOrDefault(x => x.bPrimary), ScoreCard.ScoreMetrics, EventAggregator, StructureDictionaryService);

                var newplan = normService.GetPlan();
                Plans.Add(newplan);
                Plans.FirstOrDefault(x => x.CourseId == newplan.CourseId && x.PlanId == newplan.PlanId).bSelected = true;
            }
        }

        // Initialize 
        private void InitializeClass()
        {

            // Set the PatientID
            PatientId = Plan.PatientId;
            _isWindowActive = true;
            // Add the Plans
            // Clear the Collection of Plans
            //Plans.Clear();

            // For each course, add all the Plans
            //foreach (Course course in Patient.Courses)
            //{
            //    foreach (PlanSetup plan in course.PlanSetups.Where(x=>x.StructureSet!=null))
            //        Plans.Add(new PlanModel(plan, EventAggregator));
            //}
            //if (Plan != null && Plan.StructureSet!=null)
            //{
            //    if (Plans.Any(x => x.PlanId == Plan.Id && x.CourseId == Course.Id))
            //    {
            //        Plans.FirstOrDefault(x => x.PlanId == Plan.Id && x.CourseId == Course.Id).bPrimary = true;
            //    }
            //    if(!String.IsNullOrEmpty(old_course) && !String.IsNullOrEmpty(old_plan) && Plans.Any(x=>x.PlanId == old_plan && x.CourseId == old_course))
            //    {
            //        Plans.FirstOrDefault(x => x.PlanId == Plan.Id && x.CourseId == Course.Id).bSelected = true;
            //    }
            //}

            //Plans.First().bPrimary = true;
            ScorePlan();
        }

        // Score Plan
        private void ScorePlan()
        {
            if (_isWindowActive && ScoreCard != null && Plans.Count() > 0 && Plans.Any(x => x.bSelected))
                ScorePlan(ScoreCard);

            ProgressViewService.Close();
        }

        public void ScorePlan(ScoreCardModel scoreCard)
        {
            if (EditScoreCardView != null)
            {
                InvalidateScores();
                if (EditScoreCardView.IsVisible)
                { EditScoreCardView.Hide(); }
            }

            ScoreCard = scoreCard;
            //the scorecard rx may have changed
            DosePerFraction = scoreCard.DosePerFraction;
            NumberOfFractions = scoreCard.NumberOfFractions;
            //SetRxMessage();
            ProgressViewService.ShowProgress("Scoring Plans", 100, true);
            ProgressViewService.SendToFront();

            // _eventAggregator.GetEvent<UpdateTemplatesEvent>().Publish(_currentTemplate);
            PlanScores.Clear();

            // Get Collection of SelectedPlans

            //Plans.Where(p => p.bSelected == true).Select(s => s.Plan as PlanningItem).ToList();

            // Convert the List to an Observable Collection
            //ObservableCollection<PlanningItem> selectedPlanCollection = new ObservableCollection<PlanningItem>();
            //foreach (PlanningItem plan in selectedPlans)
            //    selectedPlanCollection.Add(plan);

            // Initiate the MetricId Counter
            int metric_id = 0;

            // Loop through each Metric (ScoreTemplateModel)
            bWarning = false;
            bFlag = false;
            Warnings = String.Empty;
            Flags = String.Empty;
            foreach (ScoreTemplateModel template in scoreCard.ScoreMetrics)
            {
                //plans must be selected.
                if (!Plans.Any(pl => pl.bSelected))
                {
                    System.Threading.Thread.Sleep(700);
                    ProgressViewService.Close();
                    return;
                }
                if (!Plans.Any(pl => pl.bPrimary))
                {
                    //swap deselect so it doesn't call the scoreplan method again. 
                    Plans.FirstOrDefault(pl => pl.bSelected)._deselect = true;
                    Plans.FirstOrDefault(pl => pl.bSelected).bPrimary = true;
                    Plans.FirstOrDefault(pl => pl.bSelected)._deselect = false;
                }
                PlanScoreModel psm = null;
                if (!PlanScores.Any(ps => ps.MetricId == metric_id))
                {
                    psm = new PlanScoreModel(Application, StructureDictionaryService);
                }
                else
                {
                    psm = PlanScores.FirstOrDefault(p => p.MetricId == metric_id);
                }
                psm.InputTemplate(template);
                foreach (var patient in Plans.OrderByDescending(pl => pl.bPrimary).GroupBy(p => p.PatientId).Where(pa => pa.Any(pl => pl.bSelected)))
                {
                    List<PlanningItem> selectedPlans = GetPlansPlanModel(patient.Key);
                    // PlanScoreModel

                    //if (!Plans.Any(pl => pl.bPrimary))
                    //{
                    psm.BuildPlanScoreFromTemplate(selectedPlans, template, metric_id, Plans.FirstOrDefault(x => x.bPrimary).CourseId, Plans.FirstOrDefault(x => x.bPrimary).PlanId, true);
                    //}
                    //else if (Plans.Any(pl => pl.bSelected))
                    //{
                    //    Plans.FirstOrDefault(pl => pl.bSelected).bPrimary = true;
                    //    //psm.BuildPlanScoreFromTemplate(selectedPlanCollection, template, metric_id, Plans.FirstOrDefault(x => x.bPrimary).CourseId, Plans.FirstOrDefault(x => x.bPrimary).PlanId, true);
                    //    return;
                    //}
                    //else
                    //{
                    //System.Windows.MessageBox.Show("Must select a plan for building");

                    //}
                    if (!PlanScores.Any(ps => ps.MetricId == metric_id))
                    {
                        PlanScores.Add(psm);
                    }
                }
                psm.CheckOutsideBounds();
                if (template.ScorePoints.Any(x => x.Variation) && psm.ScoreValues.Any(x => x.Score < template.ScorePoints.FirstOrDefault(y => y.Variation).Score))
                {
                    bWarning = true;
                    foreach (var scoreBelowVariation in psm.ScoreValues.Where(x => x.Score < template.ScorePoints.FirstOrDefault(y => y.Variation).Score))
                    {
                        Warnings += $"Patient [{scoreBelowVariation.PatientId}]. Course [{scoreBelowVariation.CourseId}]. Plan [{scoreBelowVariation.PlanId}]. Metric {psm.MetricId + 1}. Structure {psm.StructureId}. -- {psm.MetricText} below variation\n";
                    }
                }
                if (psm.ScoreValues.Any(x => x.Score == 0))
                {
                    bFlag = true;
                    foreach (var zeroScore in psm.ScoreValues.Where(x => x.Score == 0))
                    {
                        Flags += $"Patient [{zeroScore.PatientId}]. Course [{zeroScore.CourseId}]. Plan [{zeroScore.PlanId}]. Metric {psm.MetricId + 1}. Structure {psm.StructureId}. -- {psm.MetricText}. Score is 0.\n";
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
                    foreach (var pc in planScores.FirstOrDefault().ScoreValues.Select(x => new { patientId = x.PatientId, planId = x.PlanId, courseId = x.CourseId }))
                    {
                        string patid = pc.patientId;
                        string cid = pc.courseId;
                        string pid = pc.planId;

                        double planTotal = planScores.Sum(x => x.ScoreValues.FirstOrDefault(y => y.PlanId == pc.planId && y.CourseId == pc.courseId && y.PatientId == pc.patientId).Score);
                        ScoreTotalText += $"\t{patid}: [{cid}] {pid}: {planTotal:F2}/{planScores.Sum(x => x.ScoreMax):F2} ({planTotal / planScores.Sum(x => x.ScoreMax) * 100.0:F2}%)\n\t";

                        PlanModel plan = Plans.FirstOrDefault(p => p.PlanId == pid && p.CourseId == cid && p.PatientId == patid);

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
            //remove patient for next time score plan is run we should access the patient again.
            _patient = null;
            _patientId = String.Empty;
        }

        private List<PlanningItem> GetPlansPlanModel(string patientId)
        {
            List<PlanningItem> planItems = new List<PlanningItem>();
            if (!patientId.Equals(_patientId))
            {
                Application.ClosePatient();
                _patient = Application.OpenPatientById(patientId);
                _patientId = patientId;
            }
            foreach (var planModel in Plans.Where(pl => pl.PatientId.Equals(_patientId)).Where(pl => pl.bSelected).OrderByDescending(pl=>pl.bPrimary))
            {
                //planModel.bPlanScoreValid = true;//make plan score visible.
                planItems.Add(_patient.Courses.FirstOrDefault(c => c.Id.Equals(planModel.CourseId)).PlanSetups.FirstOrDefault(ps => ps.Id.Equals(planModel.PlanId)));
            }
            return planItems;
        }

        private void EditScoreCard()
        {
            // Show the Progress Bar
            ProgressViewService.ShowProgress("Loading Scorecard", 100, true);

            ScoreCardModel scoreCard = new ScoreCardModel(TemplateName, TemplateSite, DosePerFraction, NumberOfFractions, ScoreCard?.ScoreMetrics);
            EditScoreCardView = ViewLauncherService.GetEditScoreCardView();

            // Events
            EventAggregator.GetEvent<EditScoreCardSetPlanEvent>().Publish(Plan); // Push the SelectedPlan
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
                        DosePerFraction = template.DosePerFraction;
                        NumberOfFractions = template.NumberOfFractions;
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
                            if (ScoreTemplates.Any(st => !String.IsNullOrEmpty(st.MetricType)))
                            {
                                importSuccess = true;
                            }
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



                }
                if (importSuccess)
                {
                    InvalidateScores();
                    ScoreCard = new ScoreCardModel(TemplateName, TemplateSite, DosePerFraction, NumberOfFractions, ScoreTemplates);

                    //any empty colors should be white.
                    if (bRxScaling && !Plan.bPlanSum && DosePerFraction != 0.0 && NumberOfFractions != 0)
                    {
                        bool bDoseMatch, bFxMatch;
                        CheckPlanRx(out bDoseMatch, out bFxMatch);
                        if (!bDoseMatch || !bFxMatch && !_bScaled)
                        {
                            ScaleScorecard(ScoreCard.NumberOfFractions * ScoreCard.DosePerFraction, Plan.NumberOfFractions * Plan.DosePerFraction, Plan.DoseUnit.ToString());
                            return;
                        }
                    }
                    bRxScaling = false;
                    EventAggregator.GetEvent<ScorePlanEvent>().Publish(ScoreCard);
                }
            }
        }

        private void InvalidateScores()
        {
            foreach(var planModel in Plans)
            {
                if (!planModel.bSelected)
                {
                    planModel.PlanScore = null;// = false;
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
                    Plan = plan;// Patient.Courses.FirstOrDefault(x => x.Id == plan.CourseId).PlanSetups.FirstOrDefault(x => x.Id == plan.PlanId && x.Course.Id == plan.CourseId);
                                //if (Plan == null)
                                //{
                                //    Plan = Patient.Courses.FirstOrDefault(x => x.Id == plan.CourseId).PlanSums.FirstOrDefault(x => x.Id == plan.PlanId && x.Course.Id == plan.CourseId);
                                //}
                                //if (Plan != null)
                                //{
                    Plans.Add(plan);
                    //}
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
        private string old_course;
        private string old_plan;
        public void UpdatePlanModel(List<PlanModel> plans)
        {
            old_course = Plans.FirstOrDefault(x => x.bPrimary)?.CourseId;//Course.Id;
            old_plan = Plans.FirstOrDefault(x => x.bPrimary)?.PlanId;
            //Patient = patient;
            //Course = course;
            //Plan = plan;
            //Plans.Clear();
            foreach(var plan in plans)
            {
                if (Plans.Any(pl => pl.PlanId.Equals(plan.PlanId) && pl.CourseId.Equals(plan.CourseId) && pl.PatientId.Equals(plan.PatientId)))
                {
                    var selectedPlan = Plans.First(pl => pl.PlanId.Equals(plan.PlanId) && pl.CourseId.Equals(plan.CourseId) && pl.PatientId.Equals(plan.PatientId));
                    if (!selectedPlan.bSelected)
                    {
                        selectedPlan.bSelected = true;
                    }
                }
                else
                {
                    Plans.Add(plan);
                }
                }
            InitializeClass();
            //OnPlanChanged(new List<PlanModel> { new PlanModel(Plan as PlanningItem, EventAggregator) { PlanId = Plan.Id, CourseId = Course.Id, bSelected = true } });
        }
    }
}
