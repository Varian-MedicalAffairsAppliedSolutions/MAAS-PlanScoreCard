using Microsoft.Win32;
using Newtonsoft.Json;
using PlanScoreCard.Events;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Events.StructureBuilder;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Services;
using PlanScoreCard.ViewModels.MetricEditors;
using PlanScoreCard.ViewModels.VMHelpers;
using PlanScoreCard.Views;
using PlanScoreCard.Views.HelperWindows;
using PlanScoreCard.Views.MetricEditors;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private ProgressViewService ProgressViewService;
        private StructureDictionaryService StructureDictionaryService;

        private object metricEdtiorControl;

        public object MetricEditorControl
        {
            get { return metricEdtiorControl; }
            set
            {
                SetProperty(ref metricEdtiorControl, value);
            }
        }
        #region visibilityBools
        public EditCIViewModel EditCIViewModel { get; set; }
        public EditDoseAtVolumeViewModel EditDoseAtVolumeViewModel { get; set; }
        public EditDoseValueViewModel EditDoseValueViewModel { get; set; }
        public EditHIViewModel EditHIViewModel { get; set; }
        public EditVolumeAtDoseViewModel EditVolumeAtDoseViewModel { get; set; }
        private bool _bCIView;

        public bool bCIView
        {
            get { return _bCIView; }
            set { SetProperty(ref _bCIView, value); }
        }
        private bool _bDAtVView;

        public bool bDAtVView
        {
            get { return _bDAtVView; }
            set { SetProperty(ref _bDAtVView, value); }
        }
        private bool _bDValueView;

        public bool bDValueView
        {
            get { return _bDValueView; }
            set { SetProperty(ref _bDValueView, value); }
        }
        private bool _bHIView;

        public bool bHIView
        {
            get { return _bHIView; }
            set { SetProperty(ref _bHIView, value); }
        }
        private bool _bVAtDView;

        public bool bVAtDView
        {
            get { return _bVAtDView; }
            set { SetProperty(ref _bVAtDView, value); }
        }

        #endregion
        public ObservableCollection<MetricTypeEnum> MetricTypes { get; set; }

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
                DosePerFraction = scoreCard.DosePerFraction;
                NumberOfFractions = scoreCard.NumberOfFractions;
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
                DeleteMetricCommand.RaiseCanExecuteChanged();
                CopyMetricCommand.RaiseCanExecuteChanged();
                MetricUpCommand.RaiseCanExecuteChanged();
                MetricDownCommand.RaiseCanExecuteChanged();
                ShowScorePointModels(SelectedScoreMetric);

                if (SelectedScoreMetric == null)
                    return;

                UpdateScoreMetricPlotModel();
                //MetricEditorControl = null;
                UpdateMetricEditor(SelectedScoreMetric);

                if (SelectedScoreMetric.Structure == null || String.IsNullOrWhiteSpace(SelectedScoreMetric.Structure?.StructureId))
                {
                    SelectedStructure = null;
                }
                else
                {

                    SelectedStructure = Structures.FirstOrDefault(x => x.StructureId.Equals(SelectedScoreMetric.Structure.StructureId, StringComparison.OrdinalIgnoreCase));
                    //SelectedScoreMetric.Structure;
                    readyEdit = true;
                }
                if (String.IsNullOrEmpty(SelectedScoreMetric.MetricComment))
                {
                    MetricComment = null;
                }
                else
                {
                    MetricComment = SelectedScoreMetric.MetricComment;
                }
            }
        }
        private bool readyEdit = false;
        private void KeepDictionaryStructure(StructureModel structureModel, string structureId)
        {
            bool structureExcluded = false;
            var configFile = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var excludeStructures = configFile.AppSettings.Settings["DictionaryExclusions"].Value.Split(';');
            if (excludeStructures != null && excludeStructures.Length > 0)
            {
                if (excludeStructures.Any(x => x.Equals(structureId, StringComparison.OrdinalIgnoreCase)))
                {
                    structureExcluded = true;
                }
            }
            if (!structureExcluded && structureModel != null)
            {
                //MessageBoxResult result = MessageBox.Show("This structure is not contained within the Structure Dictionary, would you like to add it?", "New StructureId", MessageBoxButton.YesNo);
                AskAddDictionaryStructureView = new AskAddDictionaryStructureView();
                AskAddDictionaryStructureView.DataContext = new AskAddDictionaryStructureViewModel(EventAggregator, structureId);
                AskAddDictionaryStructureView.ShowDialog();
                if (_askEditDictionary)
                {
                    StructureSelectorView = new StructureDictionarySelectorView(StructureDictionaryService, structureId, structureModel.TemplateStructureId, EventAggregator);


                    StructureSelectorView.ShowDialog();
                    //re-evaluate structures that could have been matched in the new structure dictionary. 
                }
            }
            if (structureModel != null)
            {
                foreach (var score in ScoreMetrics.Where(x => x.Structure != null && x.Structure.TemplateStructureId == structureModel.TemplateStructureId))
                {
                    if (score.Structure != null && String.IsNullOrEmpty(score.Structure.StructureId))
                    {
                        score.Structure.StructureId = structureId;
                    }
                }
            }
        }

        private void UpdateStructuresBasedOnDictionary()
        {
            foreach (var structure in ScoreMetrics.Where(x => String.IsNullOrWhiteSpace(x.Structure.StructureId)))
            {
                StructureDictionaryModel structureDictionary = StructureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.Equals(structure.Structure.TemplateStructureId, StringComparison.OrdinalIgnoreCase));
                if (structureDictionary != null && structureDictionary.StructureSynonyms != null)
                {
                    // Get a collection of all acceptable Structures
                    List<string> acceptedStructures = new List<string>();
                    //acceptedStructures.Add(structureDictionary.StructureID.ToLower());
                    if (structureDictionary.StructureSynonyms != null)
                    {
                        acceptedStructures.AddRange(structureDictionary.StructureSynonyms.Select(s => s.ToLower()));
                    }

                    // Gets the Plan Structures
                    var planStructures = Structures.Select(x => x.StructureId.ToLower());

                    // Finds any matches between the PlanStructures and All Accepted StructIDs
                    //Structure structure = null;
                    string matchedStructureID = planStructures.Intersect(acceptedStructures).FirstOrDefault();
                    if (matchedStructureID != null)
                    {
                        structure.Structure.StructureId = Structures.FirstOrDefault(x => x.StructureId.Equals(matchedStructureID, StringComparison.OrdinalIgnoreCase)).StructureId;
                        //structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id.ToLower() == matchedStructureID.ToLower());
                    }
                }
            }
        }

        // Selected Structure
        private StructureModel selectedStructure;

        public StructureModel SelectedStructure
        {
            get { return selectedStructure; }
            set
            {
                if (readyEdit && SelectedStructure == null && value != null && !String.IsNullOrWhiteSpace(value.StructureId) && String.IsNullOrEmpty(value.TemplateStructureId))
                {
                    KeepDictionaryStructure(SelectedScoreMetric.Structure, value.StructureId);
                }
                SetProperty(ref selectedStructure, value);
                OpenDictionaryEditorCommand.RaiseCanExecuteChanged();
                //&& String.IsNullOrWhiteSpace(SelectedScoreMetric.Structure.StructureId)

                //if (SelectedScoreMetric.Structure == null)
                if (SelectedStructure != null)
                {
                    if (SelectedStructure.TemplateStructureId == null)
                    {
                        SelectedStructure.TemplateStructureId = SelectedStructure.StructureId;
                    }
                    if (!String.IsNullOrWhiteSpace(SelectedStructure.StructureId) && String.IsNullOrWhiteSpace(SelectedScoreMetric.Structure?.StructureId))
                    {
                        if (SelectedScoreMetric.Structure == null)
                        {
                            SelectedScoreMetric.Structure = new StructureModel
                            {
                                StructureId = SelectedStructure.StructureId,
                                StructureCode = SelectedStructure.StructureCode,
                                StructureComment = SelectedStructure.StructureComment,
                                AutoGenerated = SelectedStructure.AutoGenerated,
                            };
                            if (String.IsNullOrEmpty(SelectedScoreMetric.Structure.TemplateStructureId))
                            {
                                SelectedScoreMetric.Structure.TemplateStructureId = selectedStructure.StructureId;
                            }
                        }
                        SelectedScoreMetric.Structure.StructureId = SelectedStructure.StructureId;

                    }
                }
                //do not change the selected score metric. This should be unique to the scorecard. 
                //SelectedScoreMetric.Structure = SelectedStructure;

                if (SelectedStructure == null)
                    return;

                // This checks to match a key to a template structure id.
                //StructureDictionaryModel dictionary = StructureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.ToLower() == SelectedStructure.TemplateStructureId.ToLower());

                // This matches values (syn)
                //if (dictionary != null)
                //{

                //}
                //if (dictionary == null)
                //{
                //    string structureID = StructureDictionaryService.FindMatch(selectedStructure.TemplateStructureId);
                //    dictionary = StructureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.ToLower() == structureID.ToLower());
                //}


                //move to structure selection. 
                /*if (dictionary == null)
                {
                    MessageBoxResult result = MessageBox.Show("This structure is not contained within the Structure Dictionary, would you like to add it?", "New StructureId", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        StructureDictionarySelectorView structureSelector = new StructureDictionarySelectorView(StructureDictionaryService, selectedStructure.StructureId, EventAggregator);
                        structureSelector.ShowDialog();
                    }

                }*/


                //if (SelectedStructure != null)
                //{
                //    if (String.IsNullOrWhiteSpace(SelectedScoreMetric.Structure.StructureId))
                //        SelectedScoreMetric.Structure.StructureId = SelectedStructure.StructureId;

                //    SelectedStructure.TemplateStructureId = selectedStructure.StructureId;
                //    SelectedStructure.AutoGenerated = SelectedScoreMetric.Structure.AutoGenerated;
                //    SelectedStructure.StructureCode = SelectedScoreMetric.Structure.StructureCode;  
                //}

            }
        }

        // ScorePoint Collection
        private ObservableCollection<ScorePointModel> metricPointModels;

        public ObservableCollection<ScorePointModel> MetricPointModels
        {
            get { return metricPointModels; }
            set
            {
                SetProperty(ref metricPointModels, value);

                //if (metricPointModels.Count() > 0)
                //    SelectedMetricPointModel = metricPointModels.First();
            }
        }

        // Selected ScorePoint
        private ScorePointModel selectedMetricPointModel;

        public ScorePointModel SelectedMetricPointModel
        {
            get { return selectedMetricPointModel; }
            set 
            { 
                SetProperty(ref selectedMetricPointModel, value);
                DeletePointCommand.RaiseCanExecuteChanged();
                PointUpCommand.RaiseCanExecuteChanged();
                PointDownCommand.RaiseCanExecuteChanged();
            }
        }

        // Structures Collection

        public ObservableCollection<StructureModel> Structures { get; private set; }

        // Template Author
        private string templateAuthor;

        public string TemplateAuthor
        {
            get { return templateAuthor; }
            set { SetProperty(ref templateAuthor, value); }
        }
        private double _dosePerFraction;

        public double DosePerFraction
        {
            get { return _dosePerFraction; }
            set 
            { 
                SetProperty(ref _dosePerFraction,value); 
                if(DosePerFraction!= ScoreCard.DosePerFraction)
                {
                    ScoreCard.DosePerFraction = DosePerFraction;
                }
            }
        }
        private int _numberOfFractions;

        public int NumberOfFractions
        {
            get { return _numberOfFractions; }
            set 
            { 
                SetProperty(ref _numberOfFractions, value); 
                if(NumberOfFractions!= ScoreCard.NumberOfFractions)
                {
                    ScoreCard.NumberOfFractions = NumberOfFractions;
                }
            }
        }

        // Template Name
        private string templateName;

        public string TemplateName
        {
            get { return templateName; }
            set { SetProperty(ref templateName, value); }
        }

        // Selected Treatment Site

        private string selectedTreatmentSite;

        public string SelectedTreatmentSite
        {
            get { return selectedTreatmentSite; }
            set { SetProperty(ref selectedTreatmentSite, value); }
        }

        private string _metricComment;

        public string MetricComment
        {
            get { return _metricComment; }
            set
            {
                SetProperty(ref _metricComment, value);
                if (!String.IsNullOrEmpty(MetricComment))
                {
                    selectedMetric.MetricComment = MetricComment;
                }
            }
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
                SetProperty(ref scoreMetricPlotModel, value);

                if (SelectedScoreMetric != null)
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
        public DelegateCommand PointUpCommand { get; private set; }
        public DelegateCommand PointDownCommand { get; private set; }
        public DelegateCommand ScorePlanCommand { get; private set; }
        public DelegateCommand SaveTemplateCommand { get; private set; }
        public DelegateCommand OrderPointsByValueCommand { get; private set; }
        public DelegateCommand AddNewStructureCommand { get; private set; }
        public StructureDictionarySelectorView StructureSelectorView { get; private set; }
        //this property is used to determine whether to open the structure dictionary editor.
        private bool _askEditDictionary { get; set; }
        public AskAddDictionaryStructureView AskAddDictionaryStructureView { get; set; }
        public DelegateCommand OpenDictionaryEditorCommand { get; private set; }
        // Constructor
        public EditScoreCardViewModel(IEventAggregator eventAggregator, 
            ViewLauncherService viewLauncherService, 
            ProgressViewService progressViewService, 
            StructureDictionaryService structureDictionaryService,
            EditCIViewModel editCIViewModel,
            EditDoseAtVolumeViewModel editDoseAtVolumeViewModel,
            EditDoseValueViewModel editDoseValueViewModel,
            EditHIViewModel editHIViewModel,
            EditVolumeAtDoseViewModel editVolumeAtDoseViewModel)
        {
            //viewmodels for modificators.
            EditCIViewModel = editCIViewModel;
            EditDoseAtVolumeViewModel = editDoseAtVolumeViewModel;
            EditDoseValueViewModel = editDoseValueViewModel;
            EditHIViewModel = editHIViewModel;
            EditVolumeAtDoseViewModel = editVolumeAtDoseViewModel;

            ViewLauncherService = viewLauncherService;
            ProgressViewService = progressViewService;
            StructureDictionaryService = structureDictionaryService;

            // Events
            EventAggregator = eventAggregator;
            EventAggregator.GetEvent<LoadEditScoreCardViewEvent>().Subscribe(LoadScoreCard);
            EventAggregator.GetEvent<EditScoreCardSetUserEvent>().Subscribe(SetUser);
            EventAggregator.GetEvent<EditScoreCardSetPlanEvent>().Subscribe(SetPlan);
            EventAggregator.GetEvent<MetricRankChangedEvent>().Subscribe(ReRankMetrics);
            EventAggregator.GetEvent<ScoreMetricPlotModelUpdatedEvent>().Subscribe(UpdateScoreMetricPlotModel);
            EventAggregator.GetEvent<ReRankMetricPointsEvent>().Subscribe(ReRankPoints);
            EventAggregator.GetEvent<UpdateScorePointGridEvent>().Subscribe(ReloadScorePoints);
            //EventAggregator.GetEvent<UpdateScorePointGridEvent>().Subscribe(ReloadScorePoints);
            EventAggregator.GetEvent<UpdateMetricEditorEvent>().Subscribe(ChangeMetricEditor);
            EventAggregator.GetEvent<AddStructureEvent>().Subscribe(AddNewStructure);
            EventAggregator.GetEvent<UpdateScroreMetricsEvent>().Subscribe(UpdateMetrics);
            EventAggregator.GetEvent<YesEvent>().Subscribe(OnDictionaryYes);
            EventAggregator.GetEvent<NoEvent>().Subscribe(OnDictionaryNo);
            //commented because structures should be matched automatically on selection without requirement of dictionary being selected.
            //EventAggregator.GetEvent<StructureDictionaryAddedEvent>().Subscribe(UpdateStructuresBasedOnDictionary);

            // Commands
            DeleteMetricCommand = new DelegateCommand(DeleteMetric, CanDeleteMetric);
            AddMetricCommand = new DelegateCommand(AddMetric);
            CopyMetricCommand = new DelegateCommand(CopyMetric, CanDeleteMetric);
            MetricUpCommand = new DelegateCommand(MetricUp, CanDeleteMetric);
            MetricDownCommand = new DelegateCommand(MetricDown, CanDeleteMetric);
            AddPointCommand = new DelegateCommand(AddPoint);
            DeletePointCommand = new DelegateCommand(DeletePoint, CanDeletePoint);
            PointUpCommand = new DelegateCommand(PointUp, CanDeletePoint);
            PointDownCommand = new DelegateCommand(PointDown, CanDeletePoint);
            ScorePlanCommand = new DelegateCommand(ScorePlan);
            SaveTemplateCommand = new DelegateCommand(SaveTemplate);
            OrderPointsByValueCommand = new DelegateCommand(OrderPointsByValue);
            AddNewStructureCommand = new DelegateCommand(OnAddNewStructure);
            OpenDictionaryEditorCommand = new DelegateCommand(OnOpenDictionaryEditor, CanOpenDictionaryEditor);

            // Inititate Collections
            Structures = new ObservableCollection<StructureModel>();
            ScoreMetrics = new ObservableCollection<ScoreMetricModel>();
            MetricPointModels = new ObservableCollection<ScorePointModel>();
            TreatmentSites = new ObservableCollection<string>();
            MetricTypes = new ObservableCollection<MetricTypeEnum>();

            Bind();
        }

        //private bool CanMetricUp()
        //{
        //    throw new NotImplementedException();
        //}

        private bool CanDeletePoint()
        {
            return SelectedScoreMetric != null && SelectedMetricPointModel != null;
        }

        private bool CanDeleteMetric()
        {
           return SelectedScoreMetric != null;
        }

        private bool CanOpenDictionaryEditor()
        {
            return SelectedStructure != null && !String.IsNullOrEmpty(SelectedStructure.StructureId) && !String.IsNullOrEmpty(SelectedStructure.TemplateStructureId);
        }

        private void OnOpenDictionaryEditor()
        {
            StructureSelectorView = new StructureDictionarySelectorView(StructureDictionaryService, SelectedStructure.StructureId, SelectedStructure.TemplateStructureId, EventAggregator);
            StructureSelectorView.ShowDialog();
        }

        private void OnDictionaryNo()
        {
            _askEditDictionary = false;
            AskAddDictionaryStructureView.DialogResult = false;
        }

        private void OnDictionaryYes()
        {
            _askEditDictionary = true;
            AskAddDictionaryStructureView.DialogResult = true;
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

        private void UpdateMetrics()
        {
            if (SelectedScoreMetric == null)
                return;

            ScoreMetrics = ScoreMetrics;

            //ScoreMetrics.Remove(ScoreMetrics.FirstOrDefault(s => s.Id == SelectedScoreMetric.Id));
            //ScoreMetrics.Add(SelectedScoreMetric);
            //ScoreMetrics.OrderBy(sm => sm.Id);
        }

        private void AddNewStructure(StructureModel structure)
        {
            Structures.Add(structure);
            Structures.OrderBy(s => s.StructureId);

            //also add structures to each metric in case you want to change it.
            //foreach (var sm in ScoreMetrics)
            //    sm.Structures.Add(structure);

        }

        private void OnAddNewStructure()
        {

            BuildStructureView builderView = ViewLauncherService.GetStructureBuilderView();
            //EventAggregator.GetEvent<SetPlanModelEvent>().Publish(PlanModel);
            //builderView.DataContext = new BuildStructureViewModel(PlanModel, EventAggregator);
            EventAggregator.GetEvent<SetStructureBuilderPlanEvent>().Publish(PlanModel);
            builderView.ShowDialog();

            // Need to do something to refresh the structures

            //StructureBuilderView structureBuilderView = new StructureBuilderView()
            //{
            //    DataContext = new StructureBuilderViewModel(_planModel, _eventAggregator)
            //};
            //structureBuilderView.ShowDialog();
            //launch viewmodel.
            //FillStructures();
        }

        private void OrderPointsByValue()
        {
            if (SelectedScoreMetric == null)
                return;

            MetricPointModels.Clear();

            List<ScorePointModel> scorePoints = SelectedScoreMetric.ScorePoints.OrderBy(sm => sm.PointX).ToList();
            foreach (ScorePointModel point in scorePoints)
                MetricPointModels.Add(point);

            ReRankPoints();

        }

        private void SaveTemplate()
        {
            List<ScoreTemplateModel> scoreTemplates = ScoreTemplateBuilder.Build(ScoreMetrics.ToList(), Structures.ToList());
            InternalTemplateModel template = new InternalTemplateModel()
            {
                Creator = TemplateAuthor,
                Site = String.IsNullOrEmpty(SelectedTreatmentSite) ? "Undefined" : SelectedTreatmentSite,
                TemplateName = String.IsNullOrEmpty(TemplateName) ? "Undefined" : TemplateName,
                DosePerFraction = DosePerFraction,
                NumberOfFractions = NumberOfFractions
            };
            template.ScoreTemplates = scoreTemplates;
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JSON Format (.json)|*.json";
            sfd.Title = "Save as PlanSC format";
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, JsonConvert.SerializeObject(template));
            }
        }

        private void ScorePlan()
        {
            var scoreTemplate = ScoreTemplateBuilder.Build(ScoreMetrics.ToList(), Structures.ToList());
            ScoreCard.ScoreMetrics = scoreTemplate;
            EventAggregator.GetEvent<ScorePlanEvent>().Publish(ScoreCard);
        }

        private void PointDown()
        {
            int selectedIndex = MetricPointModels.IndexOf(SelectedMetricPointModel);
            int maxIndex = MetricPointModels.Count();
            if (selectedIndex + 1 >= maxIndex)
                return;
            MetricPointModels.Move(selectedIndex, selectedIndex + 1);
            ReRankPoints();
        }

        private void PointUp()
        {
            int selectedIndex = MetricPointModels.IndexOf(SelectedMetricPointModel);
            int maxIndex = MetricPointModels.Count();
            if (selectedIndex == 0)
                return;
            MetricPointModels.Move(selectedIndex, selectedIndex - 1);
            ReRankPoints();
        }

        private void ReloadScorePoints(SolidColorBrush obj)
        {
            SelectedMetricPointModel.PlanScoreBackgroundColor = obj;
            ScoreMetrics.FirstOrDefault(s => s.Id == SelectedScoreMetric.Id).ScorePoints.FirstOrDefault(p => p.MetricId == SelectedMetricPointModel.MetricId).PlanScoreBackgroundColor = obj;
        }

        private void DeletePoint()
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the ScorePoint?", "Delete Point", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            //MetricPointModels.Remove(SelectedMetricPointModel);
            //SelectedScoreMetric.ScorePoints = MetricPointModels;
            //ScoreMetrics.FirstOrDefault(s => s.Id == SelectedScoreMetric.Id).ScorePoints = MetricPointModels;

            ScoreMetrics.FirstOrDefault(s => s.Id == SelectedScoreMetric.Id).ScorePoints.Remove(SelectedMetricPointModel);
            ScoreMetrics = ScoreMetrics;
            SelectedScoreMetric = SelectedScoreMetric;

            ReRankPoints();
            UpdateScoreMetricPlotModel();
        }

        private void AddPoint()
        {
            int selectedIndex = MetricPointModels.IndexOf(SelectedMetricPointModel);
            ScorePointModel metricModel = new ScorePointModel(selectedIndex + 1, selectedIndex + 1, EventAggregator);
            SelectedMetricPointModel = metricModel;
            MetricPointModels.Insert(selectedIndex + 1, metricModel);
            //SelectedScoreMetric.ScorePoints = MetricPointModels;

            ScoreMetrics.FirstOrDefault(s => s.Id == SelectedScoreMetric.Id).ScorePoints.Insert(selectedIndex + 1, metricModel);

            // THIS WORKS ^^
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
            if (SelectedScoreMetric == null)
                return;

            ScoreMetricPlotModel = SelectedScoreMetric.ScoreMetricPlotModel;
            SelectedScoreMetric.SetPlotProperties(SelectedScoreMetric.MetricType);

            foreach (ScorePointModel scorePoint in SelectedScoreMetric.ScorePoints.ToList())
            {
                SelectedScoreMetric.OnAddPlotScorePoint(SelectedScoreMetric.Id);
            }

            ScoreMetricPlotModel.InvalidatePlot(true);
        }

        // Copy Metric
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
            {
                ScorePointModel point = new ScorePointModel(scorePoint.MetricId, scorePoint.PointId, EventAggregator);
                point.Score = scorePoint.Score;
                point.PointX = scorePoint.PointX;

                PlanScoreColorModel colorModel = scorePoint.Colors;
                point.PlanScoreBackgroundColor = scorePoint.PlanScoreBackgroundColor;
                colorModel.PlanScoreBackgroundColor = scorePoint.PlanScoreBackgroundColor;
                point.Colors = colorModel;
                point.bMetricChecked = scorePoint.bMetricChecked;
                point.bMidMetric = scorePoint.bMidMetric;

                metricModel.ScorePoints.Add(point);

            }
            metricModel.ScoreMetricPlotModel = new ViewResolvingPlotModel();
            metricModel.SetPlotProperties(metricModel.MetricType);

            foreach (ScorePointModel scorePoint in metricModel.ScorePoints.ToList())
            {
                metricModel.OnAddPlotScorePoint(metricModel.Id);
            }

            ScoreMetrics.Insert(selectedIndex + 1, metricModel);
            ReRankMetrics();
        }

        // Add Metric
        private void AddMetric()
        {
            ScoreMetricModel metricModel = new ScoreMetricModel(EventAggregator);
            int selectedIndex = ScoreMetrics.IndexOf(SelectedScoreMetric);
            metricModel.Structures = Structures;
            metricModel.CanReorder = false;
            metricModel.Id = selectedIndex + 1;
            metricModel.CanReorder = true;


            ScoreMetrics.Insert(selectedIndex + 1, metricModel);
            ReRankMetrics();
        }

        // Delete Metric
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
            if (ScoreMetrics.Count() == 0)
            {
                SelectedScoreMetric = null;
                MetricPointModels.Clear();
                ScoreMetricPlotModel = null;
                MetricEditorControl = null;
            }
            else
            {
                SelectedScoreMetric = ScoreMetrics.First();

            }
        }

        // Event Methods
        private void LoadScoreCard(ScoreCardModel scoreCardModel)
        {
            ScoreCard = scoreCardModel;
        }

        private void SetPlan(PlanModel planModel)
        {
            if (planModel == null)
                return;

            PlanModel = planModel;
            foreach (var structure in PlanModel.Structures.OrderBy(s => s.StructureId))
            {
                Structures.Add(structure);
            }
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
                metric.CanReOrder = false;
                metric.PointId = rankCounter;
                rankCounter++;
                metric.CanReOrder = true;
            }

            MetricPointModels.OrderBy(o => o.PointId);

        }

        // Show Metric / Points 

        private void ShowScoreCardMetrics(List<ScoreTemplateModel> scoreMetrics)
        {
            ScoreMetrics.Clear();
            //SelectedScoreMetric = null;
            foreach (var sm in ScoreTemplateBuilder.GetScoreCardMetricsFromTemplate(scoreMetrics, EventAggregator, 0, Structures.ToList()))
            {
                //sm.SetEventAggregator(EventAggregator);
                //sm.InititatePlot();
                sm.IsLoaded = true;
                ScoreMetrics.Add(sm);
            }

            // Set the Selected ScoreMetric
            if (ScoreMetrics.Count() == 0)
            {
                SelectedScoreMetric = null;
            }
            else
            {
                SelectedScoreMetric = ScoreMetrics.First();
            }
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

        private void ChangeMetricEditor()
        {
            if (SelectedScoreMetric == null)
                return;

            SelectedScoreMetric = ScoreMetrics.FirstOrDefault(s => s.Id == SelectedScoreMetric.Id);

            if (SelectedScoreMetric == null)
                return;

            UpdateMetricEditor(SelectedScoreMetric);
        }

        private void UpdateMetricEditor(ScoreMetricModel scoreMetric)
        {
            if (scoreMetric.MetricType == MetricTypeEnum.ConformityIndex)
            {
                //EditCIView volumeAtDoseView = ViewLauncherService.GetEditMetricView_CI();
                EventAggregator.GetEvent<ShowCIMetricEvent>().Publish(scoreMetric);
                bVAtDView = bDAtVView = bHIView = bDValueView = false;
                bCIView = true;
                //MetricEditorControl = volumeAtDoseView;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.DoseAtVolume)
            {
                //EditDoseAtVolumeView volumeAtDoseView = ViewLauncherService.GetEditMetricView_DoseAtVolume();
                EventAggregator.GetEvent<ShowDoseAtVolumeMetricEvent>().Publish(scoreMetric);
                //MetricEditorControl = volumeAtDoseView;
                bCIView = bVAtDView = bHIView = bDValueView =  false;
                bDAtVView = true;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
            {
                //EditHIView volumeAtDoseView = ViewLauncherService.GetEditMetricView_HI();
                EventAggregator.GetEvent<ShowHIMetricEvent>().Publish(scoreMetric);
                //MetricEditorControl = volumeAtDoseView;
                bCIView = bDAtVView = bVAtDView = bDValueView = false;
                bHIView = true;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.MaxDose || scoreMetric.MetricType == MetricTypeEnum.MinDose || scoreMetric.MetricType == MetricTypeEnum.MeanDose)
            {
                //EditDoseValueView volumeAtDoseView = ViewLauncherService.GetEditMetricView_DoseValue();
                EventAggregator.GetEvent<ShowDoseValueMetricEvent>().Publish(scoreMetric);
                //MetricEditorControl = volumeAtDoseView;
                bCIView = bDAtVView = bVAtDView = bHIView = false;
                 bDValueView= true;
            }
            else if (scoreMetric.MetricType == MetricTypeEnum.VolumeAtDose || scoreMetric.MetricType == MetricTypeEnum.VolumeOfRegret || scoreMetric.MetricType == MetricTypeEnum.ConformationNumber)
            {
                //EditVolumeAtDoseView volumeAtDoseView = ViewLauncherService.GetEditMetricView_VolumeAtDose();
                EventAggregator.GetEvent<ShowVolumeAtDoseMetricEvent>().Publish(scoreMetric);
                //MetricEditorControl = volumeAtDoseView;
                bCIView = bDAtVView = bDValueView = bHIView  = false;
                bVAtDView = true;
            }
        }

    }
}
