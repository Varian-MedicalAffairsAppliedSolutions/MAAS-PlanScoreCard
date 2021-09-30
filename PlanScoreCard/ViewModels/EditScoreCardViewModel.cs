using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Services;
using PlanScoreCard.Views.MetricEditors;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.ViewModels
{
    public class EditScoreCardViewModel : BindableBase
    {
        // Private Class Properties
        private User User;
        private PlanModel PlanModel;
        private IEventAggregator EventAggregator;

        private ViewLauncherService ViewLauncherService;

        private object metricEdtiorControl;

        public object MetricEditorControl
        {
            get { return metricEdtiorControl; }
            set { SetProperty( ref metricEdtiorControl , value); }
        }

        public ObservableCollection<MetricTypeEnum> MetricTypes {  get; set; }
 
        // Private Class Collections

        // ScoreCard Model
        private ScoreCardModel scoreCard;

        public ScoreCardModel ScoreCard
        {
            get { return scoreCard; }
            set 
            {
                SetProperty(ref scoreCard, value);

                TemplateName = scoreCard.Name;
                SelectedTreatmentSite = scoreCard.SiteGroup;
                ShowScoreCardMetrics(scoreCard.ScoreMetrics);
            }
        }

        // ScoreMetric Collection
        private ObservableCollection<ScoreMetricModel> scoreMetrics;

        public ObservableCollection<ScoreMetricModel> ScoreMetrics
        {
            get { return scoreMetrics; }
            set { SetProperty(ref scoreMetrics, value); }
        }

        // Selected ScoreMetric
        private ScoreMetricModel selectedMetric;

        public ScoreMetricModel SelectedScoreMetric
        {
            get { return selectedMetric; }
            set 
            {
                SetProperty(ref selectedMetric, value);
                ShowScorePointModels(selectedMetric);

                if (selectedMetric == null)
                    return;
                ScoreMetricPlotModel = selectedMetric.ScoreMetricPlotModel;
                ScoreMetricPlotModel.InvalidatePlot(true);
                UpdateMetricEditor(selectedMetric);
            }
        }

        // ScorePoint Collection
        private ObservableCollection<ScorePointModel> metricPointModels;

        public ObservableCollection<ScorePointModel> MetricPointModels
        {
            get { return metricPointModels; }
            set 
            { 
                metricPointModels = value;
                
                if(metricPointModels.Count() > 0)
                    SelectedMetricPointModel = metricPointModels.First();
            }
        }

        // Selected ScorePoint
        private ScorePointModel selectedMetricPointModel;

        public ScorePointModel SelectedMetricPointModel
        {
            get { return selectedMetricPointModel; }
            set { selectedMetricPointModel = value; }
        }

        // Structures Collection
        private ObservableCollection<StructureModel> strctures;

        public ObservableCollection<StructureModel> Structures
        {
            get { return strctures; }
            set { SetProperty( ref strctures , value); }
        }

        // Template Author
        private string templateAuthor;

        public string TemplateAuthor
        {
            get { return templateAuthor; }
            set { SetProperty(ref templateAuthor, value); }
        }

        // Template Name

        private string templateName;

        public string TemplateName
        {
            get { return templateName; }
            set { SetProperty(ref templateName , value); }
        }

        // Selected Treatment Site

        private string selectedTreatmentSite;

        public string SelectedTreatmentSite
        {
            get { return selectedTreatmentSite; }
            set { SetProperty( ref selectedTreatmentSite , value); }
        }

        // Treatment Sites
        private ObservableCollection<string> treatmentSites;

        public ObservableCollection<string> TreatmentSites
        {
            get { return treatmentSites; }
            set { SetProperty(ref treatmentSites, value); }
        }

        // ScorePoint Plot
        private ViewResolvingPlotModel scoreMetricPlotModel;

        public ViewResolvingPlotModel ScoreMetricPlotModel
        {
            get { return scoreMetricPlotModel; }
            
            set 
            { 
                SetProperty( ref scoreMetricPlotModel , value);
                SelectedScoreMetric.SetPlotProperties(SelectedScoreMetric.MetricType);
            }
        }

        // Commands
        public DelegateCommand DeleteMetricCommand { get; private set; }
        public DelegateCommand AddMetricCommand { get; private set; }
        public DelegateCommand CopyMetricCommand { get; private set; }
        public DelegateCommand MetricUpCommand { get; private set; }
        public DelegateCommand MetricDownCommand { get; private set; }
        public DelegateCommand AddPointCommand { get; private set; }
        public DelegateCommand DeletePointCommand { get; private set; }

        // Constructor
        public EditScoreCardViewModel(IEventAggregator eventAggregator, ViewLauncherService viewLauncherService)
        {

            ViewLauncherService = viewLauncherService;

            // Events
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<LoadEditScoreCardViewEvent>().Subscribe(LoadScoreCard);
            EventAggregator.GetEvent<EditScoreCardSetUserEvent>().Subscribe(SetUser);
            EventAggregator.GetEvent<EditScoreCardSetPlanEvent>().Subscribe(SetPlan);
            EventAggregator.GetEvent<MetricRankChangedEvent>().Subscribe(ReRankMetrics);
            EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Subscribe(UpdateScoreMetricPlotModel);
            EventAggregator.GetEvent<ReRankMetricPointsEvent>().Subscribe(ReRankPoints);
            EventAggregator.GetEvent<UpdateScorePointGridEvent>().Subscribe(ReloadScorePoints);

            // Commands
            DeleteMetricCommand = new DelegateCommand(DeleteMetric);
            AddMetricCommand = new DelegateCommand(AddMetric);
            CopyMetricCommand = new DelegateCommand(CopyMetric);
            MetricUpCommand = new DelegateCommand(MetricUp);
            MetricDownCommand = new DelegateCommand(MetricDown);
            AddPointCommand = new DelegateCommand(AddPoint);
            DeletePointCommand = new DelegateCommand(DeletePoint);

            // Inititate Collections
            Structures = new ObservableCollection<StructureModel>();
            ScoreMetrics = new ObservableCollection<ScoreMetricModel>();
            MetricPointModels = new ObservableCollection<ScorePointModel>();
            TreatmentSites = new ObservableCollection<string>();
            MetricTypes = new ObservableCollection<MetricTypeEnum>();

            Bind();
        }

        private void ReloadScorePoints(SolidColorBrush obj)
        {
            SelectedMetricPointModel.PlanScoreBackgroundColor = obj;
        }

        private void DeletePoint()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the ScorePoint?", "Delete Point", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            MetricPointModels.Remove(SelectedMetricPointModel);
            ReRankPoints();
        }

        private void AddPoint()
        {
            int selectedIndex = MetricPointModels.IndexOf(SelectedMetricPointModel);
            ScorePointModel metricModel = new ScorePointModel(selectedIndex + 1, selectedIndex + 1, EventAggregator);
            MetricPointModels.Insert(selectedIndex + 1, metricModel);
            ReRankPoints();
        }

        private void MetricDown()
        {
            int selectedIndex = ScoreMetrics.IndexOf(SelectedScoreMetric);
            int maxIndex = ScoreMetrics.Count();
            if (selectedIndex + 1 >= maxIndex)
                return;
            ScoreMetrics.Move(selectedIndex, selectedIndex + 1);
            ReRankMetrics();
        }

        private void MetricUp()
        {
            int selectedIndex = ScoreMetrics.IndexOf(SelectedScoreMetric);
            if (selectedIndex == 0)
                return;
            ScoreMetrics.Move(selectedIndex, selectedIndex - 1);
            ReRankMetrics();
        }

        private void UpdateScoreMetricPlotModel()
        {
            if(SelectedScoreMetric == null) 
                    return;

            ScoreMetricPlotModel = SelectedScoreMetric.ScoreMetricPlotModel;
            SelectedScoreMetric.SetPlotProperties(SelectedScoreMetric.MetricType);
            
            foreach (ScorePointModel scorePoint in SelectedScoreMetric.ScorePoints.ToList())
            {
                SelectedScoreMetric.OnAddPlotScorePoint(SelectedScoreMetric.Id);
            }

            ScoreMetricPlotModel.InvalidatePlot(true);
        }

        private void CopyMetric()
        {
            int selectedIndex = ScoreMetrics.IndexOf(SelectedScoreMetric);
            ScoreMetricModel metricModel = new ScoreMetricModel(EventAggregator);
            metricModel.MetricType = SelectedScoreMetric.MetricType;
            metricModel.InputUnit = SelectedScoreMetric.InputUnit;
            metricModel.OutputUnit = SelectedScoreMetric.OutputUnit;
            metricModel.InputValue = SelectedScoreMetric.InputValue;
            metricModel.HI_Hi = SelectedScoreMetric.HI_Hi;
            metricModel.HI_Lo = SelectedScoreMetric.HI_Lo;
            metricModel.HI_Target = SelectedScoreMetric.HI_Target;
            metricModel.MetricText = SelectedScoreMetric.MetricText;

            // Structures
            foreach (StructureModel structure in selectedMetric.Structures)
                metricModel.Structures.Add(structure);

            metricModel.Structure = metricModel.Structures.FirstOrDefault(s => s.StructureId == SelectedScoreMetric.Structure.StructureId);

            // ScorePoints
            foreach (ScorePointModel scorePoint in selectedMetric.ScorePoints)
                metricModel.ScorePoints.Add(new ScorePointModel(scorePoint.MetricId, scorePoint.PointId, EventAggregator) { Score = scorePoint.Score, PointX = scorePoint.PointX, Colors = scorePoint.Colors ,  bMetricChecked = scorePoint.bMetricChecked, bMidMetric = scorePoint.bMidMetric});

            metricModel.ScoreMetricPlotModel = new ViewResolvingPlotModel();
            metricModel.SetPlotProperties(metricModel.MetricType);

            foreach  (ScorePointModel scorePoint in metricModel.ScorePoints.ToList())
            {
                metricModel.OnAddPlotScorePoint(metricModel.Id);
            }

            ScoreMetrics.Insert(selectedIndex + 1, metricModel);
            ReRankMetrics();
        }

        private void AddMetric()
        {
            ScoreMetricModel metricModel = new ScoreMetricModel(EventAggregator);
            int selectedIndex = ScoreMetrics.IndexOf(SelectedScoreMetric);

            ScoreMetrics.Insert(selectedIndex + 1, metricModel);
            ReRankMetrics();
        }

        private void DeleteMetric()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the ScoreMetric?", "Delete Metric", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            List<ScoreMetricModel> scoreMetrics = ScoreMetrics.ToList();
            scoreMetrics.Remove(SelectedScoreMetric);

            SelectedScoreMetric = scoreMetrics.FirstOrDefault();

            ScoreMetrics.Clear();
            foreach (ScoreMetricModel metric in scoreMetrics)
                ScoreMetrics.Add(metric);

            ReRankMetrics();
            SelectedScoreMetric = ScoreMetrics.First();
        }

        // Populates the View / Binds Data
        private void Bind()
        {
            // Treatment Sites
            TreatmentSites = new ObservableCollection<string>();

            foreach (var site in ConfigurationManager.AppSettings["TreatmentSites"].Split(';'))
                TreatmentSites.Add(site);

            // Add the MetricTypes
            MetricTypes.Add(MetricTypeEnum.ConformationNumber);
            MetricTypes.Add(MetricTypeEnum.ConformityIndex);
            MetricTypes.Add(MetricTypeEnum.DoseAtVolume);
            MetricTypes.Add(MetricTypeEnum.HomogeneityIndex);
            MetricTypes.Add(MetricTypeEnum.MaxDose);
            MetricTypes.Add(MetricTypeEnum.MeanDose);
            MetricTypes.Add(MetricTypeEnum.MinDose);
            MetricTypes.Add(MetricTypeEnum.Volume);
            MetricTypes.Add(MetricTypeEnum.VolumeAtDose);
            MetricTypes.Add(MetricTypeEnum.VolumeOfRegret);
            MetricTypes.Add(MetricTypeEnum.Undefined);

        }

        // Event Methods
        private void LoadScoreCard(ScoreCardModel scoreCardModel)
        {
            ScoreCard = scoreCardModel;
            //TemplateName = ScoreCard.Name;
            //SelectedTreatmentSite = ScoreCard.SiteGroup;
            //ShowScoreCardMetrics(ScoreCard.ScoreMetrics);
        }

        private void SetPlan(PlanModel planModel)
        {
            PlanModel = planModel;
            Structures = PlanModel.Structures;
        }

        private void SetUser(User user)
        {
            User = user;
            TemplateAuthor = User.Id;
        }

        private void ReRankMetrics(Dictionary<int, int> rankChange = null)
        {
            if (rankChange != null)
            {
                // Convert the One-Based Ranks from the UI or Zero-Based
                int oldRank = rankChange.First().Key - 1;
                int newRank = rankChange.First().Value - 1;

                // Checks to see if either the old or new Rank is outside the Indecies of the Collection 
                if (newRank >= ScoreMetrics.Count())
                    newRank = ScoreMetrics.Count() - 1;

                if (oldRank >= ScoreMetrics.Count())
                    oldRank = ScoreMetrics.Count() - 1;

                // Move the Object in the Collection
                ScoreMetrics.Move(oldRank, newRank);
            }

            // Re-Rank in Collection Order
            int rankCounter = 1;
            foreach (ScoreMetricModel metric in ScoreMetrics)
            {
                metric.CanReorder = false;
                metric.Id = rankCounter;
                rankCounter++;
                metric.CanReorder = true;
            }

            ScoreMetrics.OrderBy(o => o.Id);

        }

        private void ReRankPoints()
        {
            // Re-Rank in Collection Order
            int rankCounter = 1;
            foreach (ScorePointModel metric in MetricPointModels)
            {
                metric.PointId = rankCounter;
                rankCounter++;
            }

            ScoreMetrics.OrderBy(o => o.Id);
        }

        // Show Metric / Points 

        private void ShowScoreCardMetrics(List<ScoreTemplateModel> scoreMetrics)
        {
            ScoreMetrics.Clear();

            foreach (var sm in ScoreTemplateBuilder.GetScoreCardMetricsFromTemplate(scoreMetrics, EventAggregator, 0, Structures.ToList()))
            {
                //sm.SetEventAggregator(EventAggregator);
                //sm.InititatePlot();
                ScoreMetrics.Add(sm);
            }

            // Set the Selected ScoreMetric
            SelectedScoreMetric = ScoreMetrics.First();
        }

        private void ShowScorePointModels(ScoreMetricModel selectedMetric)
        {
            if (selectedMetric == null)
                return;

            MetricPointModels.Clear();

            // Set the Selected and the Visible ScorePoints
            foreach (ScorePointModel scorePoint in selectedMetric.ScorePoints)
                MetricPointModels.Add(scorePoint);

            // Set the Selected ScorePoint
            SelectedMetricPointModel = MetricPointModels.FirstOrDefault();
        }

        private void UpdateMetricEditor(ScoreMetricModel scoreMetric)
        {
            if (scoreMetric.MetricType == MetricTypeEnum.ConformityIndex)
            {
                EditCIView volumeAtDoseView = ViewLauncherService.GetEditMetricView_CI();
                EventAggregator.GetEvent<ShowCIMetricEvent>().Publish(scoreMetric);
                MetricEditorControl = volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.DoseAtVolume)
            {
                EditDoseAtVolumeView volumeAtDoseView = ViewLauncherService.GetEditMetricView_DoseAtVolume();
                EventAggregator.GetEvent<ShowDoseAtVolumeMetricEvent>().Publish(scoreMetric);
                MetricEditorControl = volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
            {
                EditHIView volumeAtDoseView = ViewLauncherService.GetEditMetricView_HI();
                EventAggregator.GetEvent<ShowHIMetricEvent>().Publish(scoreMetric);
                MetricEditorControl = volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.MaxDose || scoreMetric.MetricType == MetricTypeEnum.MinDose || scoreMetric.MetricType == MetricTypeEnum.MeanDose)
            {
                EditDoseValueView volumeAtDoseView = ViewLauncherService.GetEditMetricView_DoseValue();
                EventAggregator.GetEvent<ShowDoseValueMetricEvent>().Publish(scoreMetric);
                MetricEditorControl = volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.VolumeAtDose || scoreMetric.MetricType == MetricTypeEnum.VolumeOfRegret || scoreMetric.MetricType == MetricTypeEnum.ConformationNumber)
            {
                EditVolumeAtDoseView volumeAtDoseView = ViewLauncherService.GetEditMetricView_VolumeAtDose();
                EventAggregator.GetEvent<ShowVolumeAtDoseMetricEvent>().Publish(scoreMetric);
                MetricEditorControl = volumeAtDoseView;
            }
        }

    }
}
