using Microsoft.Win32;
using Newtonsoft.Json;
using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Models.Proknow;
using PlanScoreCard.Views;
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
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.ViewModels
{
    public class GenerateScorecardViewModel : BindableBase
    {
        #region visibilityControls
        private bool _bLocalTemplate;

        public bool bLocalTemplate
        {
            get { return _bLocalTemplate; }
            set { SetProperty(ref _bLocalTemplate, value); }
        }
        private bool _bPKTemplate;

        public bool bPKTemplate
        {
            get { return _bPKTemplate; }
            set { SetProperty(ref _bPKTemplate, value); }
        }
        private bool _bePRTempalte;

        public bool bePRTemplate
        {
            get { return _bePRTempalte; }
            set { SetProperty(ref _bePRTempalte, value); }
        }
        private bool _bTemplateOptions;

        public bool bTemplateOption
        {
            get { return _bTemplateOptions; }
            set { SetProperty(ref _bTemplateOptions, value); }
        }
        private string _templateName;

        public string TemplateName
        {
            get { return _templateName; }
            set { SetProperty(ref _templateName, value); }
        }
        private string _selectedTreatmentSite;

        public string SelectedTreatmentSite
        {
            get { return _selectedTreatmentSite; }
            set { SetProperty(ref _selectedTreatmentSite, value); }
        }
        private string _templateAuthor;

        public string TemplateAuthor
        {
            get { return _templateAuthor; }
            set { SetProperty(ref _templateAuthor, value); }
        }


        public ObservableCollection<string> TreatmentSites { get; set; }
        public ObservableCollection<string> TemplateOptions { get; set; }
        private string selectedTemplateOption;
        public string SelectedTemplateOption
        {
            get { return selectedTemplateOption; }
            set
            {
                SetProperty(ref selectedTemplateOption, value);
                if (SelectedTemplateOption != null)
                {
                    SetVisibilityBasedOnTemplateOption();

                }
            }
        }
        private List<ScoreTemplateModel> _scoreTemplates;
        private void SetVisibilityBasedOnTemplateOption()
        {
            //var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (SelectedTemplateOption == "ESAPI PlanScore")
            {
                bLocalTemplate = true;
                bPKTemplate = bePRTemplate = bTemplateOption = false;
                //configFile.AppSettings.Settings["ImportTemplate"].Value = "local";
            }
            else if (SelectedTemplateOption == "Proknow")
            {
                bPKTemplate = true;
                bLocalTemplate = bePRTemplate = bTemplateOption = false;
                //configFile.AppSettings.Settings["ImportTemplate"].Value = "pk";
            }
            else if (SelectedTemplateOption == "ePeer Review")
            {
                bePRTemplate = true;
                bLocalTemplate = bPKTemplate = bTemplateOption = false;
                //configFile.AppSettings.Settings["ImportTemplate"].Value = "ePR";
            }
            //configFile.Save(ConfigurationSaveMode.Modified);
            //ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.SectionName);
        }

        #endregion visibilityControls
        #region bVis
        private bool _bVAtD;

        public bool BVAtD
        {
            get { return _bVAtD; }
            set { SetProperty(ref _bVAtD, value); }
        }
        private bool _bDAtV;

        public bool BDAtV
        {
            get { return _bDAtV; }
            set { SetProperty(ref _bDAtV, value); }
        }
        private bool _bDV;

        public bool BDV
        {
            get { return _bDV; }
            set { SetProperty(ref _bDV, value); }
        }
        private bool _bHI;

        public bool BHI
        {
            get { return _bHI; }
            set { SetProperty(ref _bHI, value); }
        }
        private bool _bCI;

        public bool BCI
        {
            get { return _bCI; }
            set { SetProperty(ref _bCI, value); }
        }


        #endregion bVis
        private PlanModel _planModel;
        private IEventAggregator _eventAggregator;
        private List<ScoreTemplateModel> _importScoreTemplates;

        public ObservableCollection<StructureModel> Structures { get; set; }
        private StructureModel _selectedStructure;

        public StructureModel SelectedStructure
        {
            get { return _selectedStructure; }
            set
            {
                SetProperty(ref _selectedStructure, value);
                AddMetricCommand.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<string> MetricTypes { get; set; }
        private DoseAtVolumeViewModel _doseAtVolumeViewModel;

        public DoseAtVolumeViewModel DoseAtVolumeViewModel
        {
            get { return _doseAtVolumeViewModel; }
            set { SetProperty(ref _doseAtVolumeViewModel, value); }
        }

        //public DoseAtVolumeViewModel DoseAtVolumeViewModel { get; }
        public VolumeAtDoseViewModel VolumeAtDoseViewModel { get; }
        public DoseValueViewModel DoseValueViewModel { get; }
        public HIViewModel HIViewModel { get; }
        public CIViewModel CIViewModel { get; }

        private string _selectedMetric;

        public string SelectedMetric
        {
            get { return _selectedMetric; }
            set
            {
                SetProperty(ref _selectedMetric, value);
                if (SelectedMetric == "Dose at Volume")
                {
                    BDAtV = true;
                    BVAtD = BDV = BHI = BCI = false;
                }
                else if (SelectedMetric == "Volume at Dose" || SelectedMetric == "Volume of Regret" || SelectedMetric == "Conformation Number")
                {
                    BVAtD = true;
                    BDAtV = BDV = BHI = BCI = false;
                    //only allow conformation Number to have CC.
                    if (SelectedMetric == "Conformation Number")
                    {
                        VolumeAtDoseViewModel.VolumeUnits.Clear();
                        VolumeAtDoseViewModel.VolumeUnits.Add("CC");
                        VolumeAtDoseViewModel.SelectedVolumeUnit = "CC";
                    }
                    else
                    {
                        VolumeAtDoseViewModel.VolumeUnits.Clear();
                        foreach (var vunit in ConfigurationManager.AppSettings["VolumeUnits"].Split(';'))
                        {
                            VolumeAtDoseViewModel.VolumeUnits.Add(vunit);
                        }
                        VolumeAtDoseViewModel.SelectedVolumeUnit = VolumeAtDoseViewModel.VolumeUnits.First();
                    }
                }
                else if (SelectedMetric == "Min Dose" || SelectedMetric == "Max Dose" || SelectedMetric == "Mean Dose")
                {
                    BDV = true;
                    BVAtD = BDAtV = BCI = BHI = false;
                }
                else if (SelectedMetric == "Homogeneity Index")
                {
                    BHI = true;
                    BDV = BVAtD = BCI = BDAtV = false;
                }
                else if (SelectedMetric == "Conformity Index")
                {
                    BCI = true;
                    BVAtD = BDAtV = BHI = BDV = false;
                }

                AddMetricCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<ScoreMetricViewModel> ScoreMetrics { get; set; }
        private ScoreMetricViewModel _selectedScoreMetric;

        public ScoreMetricViewModel SelectedScoreMetric
        {
            get { return _selectedScoreMetric; }
            set { SetProperty(ref _selectedScoreMetric, value); }
        }

        public DelegateCommand ImportScorecardCommand { get; private set; }
        public DelegateCommand AddMetricCommand { get; private set; }
        public DelegateCommand SaveTemplateCommand { get; private set; }
        public DelegateCommand ScorePlanCommand { get; private set; }
        public DelegateCommand ImportPKScorecardCommand { get; private set; }
        public DelegateCommand ImportEPRScorecardCommand { get; private set; }
        public DelegateCommand SetButtonVisibilityCommand { get; private set; }
        public DelegateCommand StructureBuilderCommand { get; private set; }
        public GenerateScorecardViewModel(PlanModel pm,
            List<ScoreTemplateModel> scoreTemplates,
            string templateName,
            string templateSite,
            User user,
            IEventAggregator eventAggregator)
        {
            _planModel = pm;
            _eventAggregator = eventAggregator;
            _importScoreTemplates = scoreTemplates;
            _eventAggregator.GetEvent<DeleteMetricEvent>().Subscribe(OnDeleteMetric);
            _eventAggregator.GetEvent<CopyMetricEvent>().Subscribe(OnCopyMetric);
            _eventAggregator.GetEvent<MetricUpEvent>().Subscribe(OnMetricUp);
            _eventAggregator.GetEvent<MetricDownEvent>().Subscribe(OnMetricDown);
            _eventAggregator.GetEvent<PlanChangedEvent>().Subscribe(OnPlanChanged);
            _eventAggregator.GetEvent<AddStructureEvent>().Subscribe(OnAddStructure);
            Structures = new ObservableCollection<StructureModel>();
            MetricTypes = new ObservableCollection<string>();
            ScoreMetrics = new ObservableCollection<ScoreMetricViewModel>();
            DoseAtVolumeViewModel = new DoseAtVolumeViewModel();
            VolumeAtDoseViewModel = new VolumeAtDoseViewModel();
            DoseValueViewModel = new DoseValueViewModel();
            HIViewModel = new HIViewModel();
            CIViewModel = new CIViewModel();
            //ImportScorecardCommand = new DelegateCommand(OnImportScoreCard);
            AddMetricCommand = new DelegateCommand(OnAddMetric, CanAddMetric);
            SaveTemplateCommand = new DelegateCommand(OnSaveTemplate, CanSaveTemplate);
            ScorePlanCommand = new DelegateCommand(OnScorePlan, CanScorePlan);
            //ImportPKScorecardCommand = new DelegateCommand(OnImportPKScorecard);
            //ImportEPRScorecardCommand = new DelegateCommand(OnImportEPRScorecard);
            //SetButtonVisibilityCommand = new DelegateCommand(OnSetImportVisibilty);
            StructureBuilderCommand = new DelegateCommand(OnLaunchStructureBuilder);
            //TemplateOptions = new ObservableCollection<string>();
            //TemplateOptions.Add("ESAPI PlanScore");
            //TemplateOptions.Add("Proknow");
            //TemplateOptions.Add("ePeer Review");
            TreatmentSites = new ObservableCollection<string>();
            foreach (var site in ConfigurationManager.AppSettings["TreatmentSites"].Split(';'))
            {
                TreatmentSites.Add(site);
            }
            TemplateAuthor = user.Id;
            if (templateName != null) { TemplateName = templateName; }
            if(templateSite != null) { SelectedTreatmentSite = templateSite; }
            SetInitialVisibilities();
            FillStructures();
            FillMetrics();
            if (_importScoreTemplates != null)
            {
                SetInitialScoreMetrics();
            }

        }

        private void OnAddStructure(StructureModel obj)
        {
            Structures.Add(obj);
        }

        private void OnPlanChanged(List<PlanModel> obj)
        {
            FillStructures();
        }

        private void OnLaunchStructureBuilder()
        {
            SelectedStructure = null;
            //Structures.Clear();
            StructureBuilderView structureBuilderView = new StructureBuilderView()
            {
                DataContext = new StructureBuilderViewModel(_planModel, _eventAggregator)
            };
            structureBuilderView.ShowDialog();
            //launch viewmodel.
            //FillStructures();
        }
        #region ImportMethods
        //private void OnSetImportVisibilty()
        //{
        //    bTemplateOption = true;
        //}
        //private void OnImportEPRScorecard()
        //{
        //    OpenFileDialog ofd = new OpenFileDialog();
        //    ofd.Filter = "CSV (*.csv)|*.csv";
        //    ofd.Title = "Open ePeerReview Template";
        //    if (ofd.ShowDialog() == true)
        //    {
        //        _scoreTemplates = EPeerReviewScoreModel.GetScoreTemplateFromCSV(ofd.FileName);
        //        //_eventAggregator.GetEvent<ScorePlanEvent>().Publish(_scoreTemplates);
        //        int score_newId = ScoreMetrics.Count();
        //        BuildScoreMetrics(score_newId, _scoreTemplates);
        //    }
        //}

        //private void OnImportPKScorecard()
        //{
        //    //ScoreMetrics.Clear();//
        //    OpenFileDialog ofd = new OpenFileDialog();
        //    ofd.Filter = "JSON Template (*.json)|*.json";
        //    ofd.Title = "Open ProKnow template";
        //    //nt score_newId = 0;// ScoreMetrics.Count();

        //    if (ofd.ShowDialog() == true)
        //    {
        //        PKModel pk_scoreTemplates = JsonConvert.DeserializeObject<PKModel>(File.ReadAllText(ofd.FileName));
        //        int score_newId = ScoreMetrics.Count();
        //        _scoreTemplates = pk_scoreTemplates.ConvertToTemplate();
        //        BuildScoreMetrics(score_newId, _scoreTemplates);
        //        //_eventAggregator.GetEvent<ScorePlanEvent>().Publish(_scoreTemplates);
        //    }
        //}
        ///// <summary>
        ///// Allows for the import of a scorecard. 
        ///// This will add metrics to an already existing scorecard if one is open. 
        ///// </summary>
        //private void OnImportScoreCard()
        //{
        //    //ScoreMetrics.Clear();//
        //    OpenFileDialog ofd = new OpenFileDialog();
        //    ofd.Filter = "JSON Template (*.json)|*.json";
        //    ofd.Title = "Open PlanSC template";
        //    int score_newId = ScoreMetrics.Count();
        //    if (ofd.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            InternalTemplateModel template = JsonConvert.DeserializeObject<InternalTemplateModel>(File.ReadAllText(ofd.FileName));
        //            List<ScoreTemplateModel> scoreTemplates = template.ScoreTemplates;
        //            BuildScoreMetrics(score_newId, scoreTemplates);
        //            ScorePlanCommand.RaiseCanExecuteChanged();
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                List<ScoreTemplateModel> scoreTemplates = JsonConvert.DeserializeObject<List<ScoreTemplateModel>>(File.ReadAllText(ofd.FileName));
        //                BuildScoreMetrics(score_newId, scoreTemplates);
        //                ScorePlanCommand.RaiseCanExecuteChanged();
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new ApplicationException(ex.Message);
        //            }
        //        }

        //    }
        //}
        #endregion ImportMethods
        private void SetInitialVisibilities()
        {
            switch (ConfigurationManager.AppSettings["ImportTemplate"])
            {
                case "local":
                    SelectedTemplateOption = "ESAPI PlanScore";
                    return;
                case "pk":
                    SelectedTemplateOption = "Proknow";
                    return;
                case "ePR":
                    SelectedTemplateOption = "ePeer Review";
                    return;
                default:
                    SelectedTemplateOption = "ESAPI PlanScore";
                    return;
            }
        }
        private void SetInitialScoreMetrics()
        {
            foreach (var sm in ScoreTemplateBuilder.GetScoreMetricsFromTemplate(_importScoreTemplates, _eventAggregator, 0, Structures.ToList()))
            {
                ScoreMetrics.Add(sm);
            }
            ScorePlanCommand.RaiseCanExecuteChanged();
        }

        private void OnScorePlan()
        {
            var scoreTemplate = ScoreTemplateBuilder.Build(ScoreMetrics.ToList(), Structures.ToList());
            _eventAggregator.GetEvent<ScorePlanEvent>().Publish(scoreTemplate);
        }

        private bool CanScorePlan()
        {
            return ScoreMetrics.Count() > 0;
        }

        private void OnCopyMetric(ScoreMetricViewModel scoreMetric)
        {
            int id = ScoreMetrics.Count() == 0 ? 0 : ScoreMetrics.Max(x => x.MetricId) + 1;
            ScoreMetricViewModel sMetric = null;



            //the following if statements will build new score models based on the event type. 
            //the other viewmodels are null --> MCS. 2.7.21.
            if (scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.VolumeAtDose
                || scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.VolumeOfRegret
                || scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.ConformationNumber)
            {
                sMetric = new ScoreMetricViewModel(null,
                    new VolumeAtDoseViewModel()
                    {
                        Dose = scoreMetric.ScoreMetric.InputValue,
                        SelectedDoseUnit = scoreMetric.ScoreMetric.InputUnit,
                        SelectedVolumeUnit = scoreMetric.ScoreMetric.OutputUnit
                    },
                    null,
                    null,
                    null,
                    scoreMetric.ScoreMetric.MetricType,
                    id,
                    Structures.First(x => x.StructureId == scoreMetric.StructureTxt.StructureId),
                    Structures,
                    _eventAggregator);
            }
            else if (scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.DoseAtVolume)
            {
                sMetric = new ScoreMetricViewModel(new DoseAtVolumeViewModel()
                {
                    Volume = scoreMetric.ScoreMetric.InputValue,
                    SelectedVolumeUnit = scoreMetric.ScoreMetric.InputUnit,
                    SelectedDoseUnit = scoreMetric.ScoreMetric.OutputUnit
                },
                    null, null, null, null,
                    scoreMetric.ScoreMetric.MetricType,
                    id,
                    Structures.First(x => x.StructureId == scoreMetric.StructureTxt.StructureId),
                    Structures,
                    _eventAggregator);
            }
            else if (scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.HomogeneityIndex)
            {
                sMetric = new ScoreMetricViewModel(null, null, null,
                    new HIViewModel()
                    {
                        HI_HiValue = Convert.ToDouble(scoreMetric.ScoreMetric.HI_Hi),
                        HI_LowValue = Convert.ToDouble(scoreMetric.ScoreMetric.HI_Lo)
                    },
                    null,
                    scoreMetric.ScoreMetric.MetricType,
                    id,
                    Structures.First(x => x.StructureId == scoreMetric.StructureTxt.StructureId),
                    Structures,
                    _eventAggregator);
            }
            else if (scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.MaxDose ||
                scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.MinDose ||
            scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.MeanDose)
            {
                sMetric = new ScoreMetricViewModel(null,
                    null,
                    new DoseValueViewModel()
                    {
                        SelectedDoseUnit = scoreMetric.ScoreMetric.OutputUnit
                    },
                    null, null,
                    scoreMetric.ScoreMetric.MetricType,
                    id,
                    Structures.First(x => x.StructureId == scoreMetric.StructureTxt.StructureId),
                    Structures,
                    _eventAggregator);
            }
            else if (scoreMetric.ScoreMetric.MetricType == MetricTypeEnum.ConformityIndex)
            {
                sMetric = new ScoreMetricViewModel(null, null, null, null,
                    new CIViewModel()
                    {
                        Dose = scoreMetric.ScoreMetric.InputValue,
                        SelectedDoseUnit = scoreMetric.ScoreMetric.InputUnit
                    },
                    scoreMetric.ScoreMetric.MetricType,
                    id,
                    Structures.First(x => x.StructureId == scoreMetric.StructureTxt.StructureId),
                    Structures,
                    _eventAggregator);
            }
            if (sMetric.ScoreMetric == null) { return; }//could not match metric.
            foreach (var point in scoreMetric.ScoreMetric.ScorePoints)
            {
                var metricPoint = new ScorePointModel(id, point.PointId, _eventAggregator);
                metricPoint.PointX = point.PointX;
                metricPoint.Score = point.Score;
                metricPoint.bMidMetric = point.bMidMetric;
                metricPoint.bMetricChecked = point.bMetricChecked;
                if (point.Colors.Colors.Count() == 3)
                {
                    metricPoint.Colors = new PlanScoreColorModel(new List<double>{point.Colors.Colors.First(),
                    point.Colors.Colors.ElementAt(1),point.Colors.Colors.ElementAt(2) }, point.Colors.ColorLabel);
                }
                //metricPoint.BackGroundBrush = point.BackGroundBrush;
                sMetric.ScoreMetric.ScorePoints.Add(metricPoint);
                sMetric.OnAddPlotScorePoint(id);
            }
            ScoreMetrics.Add(sMetric);
        }

        private void OnMetricDown(int obj)
        {
            var scoreMetrics_temp = new List<ScoreMetricViewModel>();
            foreach (var metric in ScoreMetrics)
            {
                if (metric.MetricId == obj + 1)
                {
                    scoreMetrics_temp.Add(metric);
                    scoreMetrics_temp.Last().MetricId--;
                }
                else if (metric.MetricId == obj)
                {
                    scoreMetrics_temp.Add(metric);
                    scoreMetrics_temp.Last().MetricId++;
                }
                else
                {
                    scoreMetrics_temp.Add(metric);
                }
            }
            ScoreMetrics.Clear();
            foreach (var metric in scoreMetrics_temp.OrderBy(x => x.MetricId))
            {
                ScoreMetrics.Add(metric);
            }
        }

        private void OnMetricUp(int obj)
        {
            var scoreMetrics_temp = new List<ScoreMetricViewModel>();
            foreach (var metric in ScoreMetrics)
            {
                if (metric.MetricId == obj - 1)
                {
                    scoreMetrics_temp.Add(metric);
                    scoreMetrics_temp.Last().MetricId++;
                }
                else if (metric.MetricId == obj)
                {
                    scoreMetrics_temp.Add(metric);
                    scoreMetrics_temp.Last().MetricId--;
                }
                else
                {
                    scoreMetrics_temp.Add(metric);
                }
            }
            ScoreMetrics.Clear();
            foreach (var metric in scoreMetrics_temp.OrderBy(x => x.MetricId))
            {
                ScoreMetrics.Add(metric);
            }
        }

        private void OnDeleteMetric(int obj)
        {
            if (ScoreMetrics.Any(x => x.MetricId == obj))
            {
                if (ScoreMetrics.First(x => x.MetricId == obj).ScoreMetric != null && ScoreMetrics.FirstOrDefault(x => x.MetricId == obj).ScoreMetric?.ScorePoints != null)
                {
                    ScoreMetrics.FirstOrDefault(x => x.MetricId == obj).ScoreMetric.ScorePoints.Clear();
                }
                ScoreMetrics.Remove(ScoreMetrics.FirstOrDefault(x => x.MetricId == obj));
            }
            ScorePlanCommand.RaiseCanExecuteChanged();
        }


        private void BuildScoreMetrics(int score_newId, List<ScoreTemplateModel> scoreTemplates)
        {
            foreach (var sm in ScoreTemplateBuilder.GetScoreMetricsFromTemplate(scoreTemplates, _eventAggregator, score_newId, Structures.ToList()))
            {
                ScoreMetrics.Add(sm);
            }
            ScorePlanCommand.RaiseCanExecuteChanged();
        }

        private void OnSaveTemplate()
        {
            //construct collection for saving. 
            List<ScoreTemplateModel> scoreTemplates = ScoreTemplateBuilder.Build(ScoreMetrics.ToList(), Structures.ToList());
            InternalTemplateModel template = new InternalTemplateModel()
            {
                Creator = TemplateAuthor,
                Site = String.IsNullOrEmpty(SelectedTreatmentSite) ? "Undefined" : SelectedTreatmentSite,
                TemplateName = String.IsNullOrEmpty(TemplateName) ? "Undefined" : TemplateName
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
        private bool CanSaveTemplate()
        {
            return true;
        }

        private void OnAddMetric()
        {
            //First check all conditions are met.
            var metricType = GetMetricType();
            int id = ScoreMetrics.Count() == 0 ? 0 : ScoreMetrics.Max(x => x.MetricId) + 1;
            ScoreMetrics.Add(new ScoreMetricViewModel(DoseAtVolumeViewModel,
                VolumeAtDoseViewModel,
                DoseValueViewModel,
                HIViewModel,
                CIViewModel,
                metricType, id, SelectedStructure, Structures, _eventAggregator));
            #region comments
            //if (SelectedMetric == "Dose at Volume")
            //{
            //    if (DoseAtVolumeViewModel.SelectedDoseUnit != null && DoseAtVolumeViewModel.SelectedVolumeUnit != null &&
            //        !String.IsNullOrEmpty(DoseAtVolumeViewModel.Volume))
            //    {
            //        //dose volume should be ready to go.
            //        int id = ScoreMetrics.Count() == 0 ? 0 : ScoreMetrics.Max(x => x.MetricId) + 1;
            //        ScoreMetrics.Add(new ScoreMetricViewModel(DoseAtVolumeViewModel, id, SelectedStructure, Structures, _eventAggregator));

            //    }
            //}
            //else if (SelectedMetric == "Volume at Dose" || SelectedMetric == "Volume of Regret")
            //{
            //    if (VolumeAtDoseViewModel.SelectedDoseUnit != null && VolumeAtDoseViewModel.SelectedVolumeUnit != null
            //        && !String.IsNullOrEmpty(VolumeAtDoseViewModel.Dose))
            //    {
            //        int id = ScoreMetrics.Count() == 0 ? 0 : ScoreMetrics.Max(x => x.MetricId) + 1;
            //        ScoreMetrics.Add(new ScoreMetricViewModel(VolumeAtDoseViewModel, 
            //            id, 
            //            SelectedStructure, 
            //            Structures, 
            //            SelectedMetric,
            //            _eventAggregator));
            //    }
            //}
            //else if (SelectedMetric == "Min Dose" || SelectedMetric == "Max Dose" || SelectedMetric == "Mean Dose")
            //{
            //    if (DoseValueViewModel.SelectedDoseUnit != null)
            //    {
            //        int id = ScoreMetrics.Count() == 0 ? 0 : ScoreMetrics.Max(x => x.MetricId) + 1;
            //        ScoreMetrics.Add(new ScoreMetricViewModel(DoseValueViewModel, id, SelectedStructure, Structures, SelectedMetric, _eventAggregator));
            //    }
            //}
            //check to see if a plan can be scored. 
            #endregion comments
            ScorePlanCommand.RaiseCanExecuteChanged();
        }

        private MetricTypeEnum GetMetricType()
        {
            switch (SelectedMetric)
            {
                case "Dose at Volume":
                    return MetricTypeEnum.DoseAtVolume;
                case "Volume at Dose":
                    return MetricTypeEnum.VolumeAtDose;
                case "Max Dose":
                    return MetricTypeEnum.MaxDose;
                case "Min Dose":
                    return MetricTypeEnum.MinDose;
                case "Mean Dose":
                    return MetricTypeEnum.MeanDose;
                case "Volume of Regret":
                    return MetricTypeEnum.VolumeOfRegret;
                case "Conformation Number":
                    return MetricTypeEnum.ConformationNumber;
                case "Homogeneity Index":
                    return MetricTypeEnum.HomogeneityIndex;
                case "Conformity Index":
                    return MetricTypeEnum.ConformityIndex;
                default:
                    return MetricTypeEnum.Undefined;
            }
        }

        private bool CanAddMetric()
        {
            return SelectedMetric != null && SelectedStructure != null;
        }

        private void FillStructures()
        {
            foreach (var structure in _planModel.Structures)
            {
                Structures.Add(structure);
            }
        }

        private void FillMetrics()
        {
            string[] metric_string = new string[] { "Min Dose",
                "Max Dose",
                "Mean Dose",
                "Dose at Volume",
                "Volume at Dose",
                "Volume of Regret",
                "Conformation Number",
                "Homogeneity Index",
                "Conformity Index"
            };
            foreach (var ms in metric_string)
            {
                MetricTypes.Add(ms);
            }
        }
    }
}
