using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Services;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.ViewModels
{
    public class EditScoreCardViewModel : BindableBase
    {
        // Private Class Properties
        private User User;
        private PlanModel PlanModel;
        private IEventAggregator EventAggregator;
 
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
                ScoreMetricPlotModel = selectedMetric.ScoreMetricPlotModel;
                ScoreMetricPlotModel.InvalidatePlot(true);
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
            set { SetProperty( ref scoreMetricPlotModel , value); }
        }

        // Constructor
        public EditScoreCardViewModel(IEventAggregator eventAggregator)
        {
            // Events
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<LoadEditScoreCardViewEvent>().Subscribe(LoadScoreCard);
            EventAggregator.GetEvent<EditScoreCardSetUserEvent>().Subscribe(SetUser);
            EventAggregator.GetEvent<EditScoreCardSetPlanEvent>().Subscribe(SetPlan);
            EventAggregator.GetEvent<MetricRankChangedEvent>().Subscribe(ReRankMetrics);

            // Inititate Collections
            Structures = new ObservableCollection<StructureModel>();
            ScoreMetrics = new ObservableCollection<ScoreMetricModel>();
            MetricPointModels = new ObservableCollection<ScorePointModel>();
            TreatmentSites = new ObservableCollection<string>();

            Bind();

        }

        // Populates the View / Binds Data
        private void Bind()
        {
            // Treatment Sites
            TreatmentSites = new ObservableCollection<string>();

            foreach (var site in ConfigurationManager.AppSettings["TreatmentSites"].Split(';'))
                TreatmentSites.Add(site);
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

        private void ReRankMetrics(Dictionary<int, int> rankChange)
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
            SelectedMetricPointModel = MetricPointModels.First();
        }

    }
}
