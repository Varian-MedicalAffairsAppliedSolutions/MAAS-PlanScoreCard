using PlanScoreCard.Events;
using PlanScoreCard.Models;
using PlanScoreCard.Models.ModelHelpers;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static PlanScoreCard.Services.StructureGenerationService;

namespace PlanScoreCard.ViewModels
{
    public class SimpleStructureBuilderViewModel : BindableBase
    {
        private SimpleStepModel _selectedBaseStep;
        private PlanModel _plan;
        private IEventAggregator _eventAggregator;

        public SimpleStepModel SelectedBaseStep
        {
            get { return _selectedBaseStep; }
            set
            {
                SetProperty(ref _selectedBaseStep, value);
                //AddBaseCommand.RaiseCanExecuteChanged();
                BaseUpCommand.RaiseCanExecuteChanged();
                BaseDownCommand.RaiseCanExecuteChanged();
                BaseDeleteCommand.RaiseCanExecuteChanged();
            }
        }
        private SimpleStepModel _selectedTargetStep;

        public SimpleStepModel SelectedTargetStep
        {
            get { return _selectedTargetStep; }
            set
            {
                SetProperty(ref _selectedTargetStep, value);
                TargetUpCommand.RaiseCanExecuteChanged();
                TargetDownCommand.RaiseCanExecuteChanged();
                TargetDeleteCommand.RaiseCanExecuteChanged();
            }
        }
        private SimpleStepModel _selectedComboStep;

        public SimpleStepModel SelectedComboStep
        {
            get { return _selectedComboStep; }
            set
            {
                SetProperty(ref _selectedComboStep, value);
                ComboUpCommand.RaiseCanExecuteChanged();
                ComboDownCommand.RaiseCanExecuteChanged();
                ComboDeleteCommand.RaiseCanExecuteChanged();
            }
        }

        private string _selectedStructureOperation;

        public string SelectedStructureOperation
        {
            get { return _selectedStructureOperation; }
            set
            {
                SetProperty(ref _selectedStructureOperation, value);
                UpdateOperationImage();
            }
        }
        private string _selectedComboOperation;

        public string SelectedComboOperation
        {
            get { return _selectedComboOperation; }
            set
            {
                SetProperty(ref _selectedComboOperation, value);
                UpdateComboImage();
            }
        }

        private string _operationImageSource;

        public string OperationImageSource
        {
            get { return _operationImageSource; }
            set { SetProperty(ref _operationImageSource, value); }
        }
        private string _comboImageSource;

        public string ComboImageSource
        {
            get { return _comboImageSource; }
            set { SetProperty(ref _comboImageSource, value); }
        }

        private string _baseMargin;

        public string BaseMargin
        {
            get { return _baseMargin; }
            set
            {
                SetProperty(ref _baseMargin, value);
                bGrouped = false;
            }
        }
        private string _targetMargin;

        public string TargetMargin
        {
            get { return _targetMargin; }
            set
            {
                SetProperty(ref _targetMargin, value);
                bGrouped = false;
            }
        }
        private string _summaryMargin;

        public string SummaryMargin
        {
            get { return _summaryMargin; }
            set { SetProperty(ref _summaryMargin, value); }
        }
        private string _comboMargin;

        public string ComboMargin
        {
            get { return _comboMargin; }
            set { SetProperty(ref _comboMargin, value); }
        }

        private string _baseSummary;

        public string BaseSummary
        {
            get { return _baseSummary; }
            set { SetProperty(ref _baseSummary, value); }
        }
        private string _targetSummary;

        public string TargetSummary
        {
            get { return _targetSummary; }
            set { SetProperty(ref _targetSummary, value); }
        }

        private bool _bGrouped;

        public bool bGrouped
        {
            get { return _bGrouped; }
            set { SetProperty(ref _bGrouped, value); }
        }
        private string _newStructureId;

        public string NewStructureId
        {
            get { return _newStructureId; }
            set { SetProperty(ref _newStructureId, value); }
        }



        public ObservableCollection<SimpleStepModel> BaseSteps { get; private set; }
        public ObservableCollection<SimpleStepModel> TargetSteps { get; private set; }
        public ObservableCollection<string> StructureOperations { get; private set; }
        public ObservableCollection<string> ComboOperations { get; private set; }
        public ObservableCollection<SimpleStepModel> ComboSteps { get; private set; }
        //Commands
        public DelegateCommand AddBaseCommand { get; private set; }
        public DelegateCommand BaseDownCommand { get; private set; }
        public DelegateCommand BaseUpCommand { get; private set; }
        public DelegateCommand BaseDeleteCommand { get; private set; }
        public DelegateCommand AddTargetCommand { get; private set; }
        public DelegateCommand TargetDownCommand { get; private set; }
        public DelegateCommand TargetUpCommand { get; private set; }
        public DelegateCommand TargetDeleteCommand { get; private set; }
        public DelegateCommand GroupStepsCommand { get; private set; }
        public DelegateCommand AddComboCommand { get; private set; }
        public DelegateCommand ComboUpCommand { get; private set; }
        public DelegateCommand ComboDownCommand { get; private set; }
        public DelegateCommand ComboDeleteCommand { get; private set; }
        public DelegateCommand CancelStructureCreationCommand { get; private set; }
        public DelegateCommand SaveStructureCreationCommand { get; private set; }
        public SimpleStructureBuilderViewModel(StructureModel structure, PlanModel plan, IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            BaseSteps = new ObservableCollection<SimpleStepModel>();
            TargetSteps = new ObservableCollection<SimpleStepModel>();
            ComboSteps = new ObservableCollection<SimpleStepModel>();
            StructureOperations = new ObservableCollection<string>() {String.Empty, "AND", "OR", "SUB" };
            ComboOperations = new ObservableCollection<string>() { String.Empty, "AND", "OR", "SUB" };
            AddBaseCommand = new DelegateCommand(OnAddBase);
            BaseDownCommand = new DelegateCommand(OnBaseDown, CanBaseMove);
            BaseUpCommand = new DelegateCommand(OnBaseUp, CanBaseMove);
            BaseDeleteCommand = new DelegateCommand(OnBaseDelete, CanBaseMove);
            AddTargetCommand = new DelegateCommand(OnAddTarget);
            TargetDownCommand = new DelegateCommand(OnTargetDown, CanTargetMove);
            TargetUpCommand = new DelegateCommand(OnTargetUp, CanTargetMove);
            TargetDeleteCommand = new DelegateCommand(OnTargetDelete, CanTargetMove);
            GroupStepsCommand = new DelegateCommand(OnGroupSteps);
            AddComboCommand = new DelegateCommand(OnAddCombo);
            ComboDownCommand = new DelegateCommand(OnComboDown, CanMoveCombo);
            ComboUpCommand = new DelegateCommand(OnComboUp, CanMoveCombo);
            ComboDeleteCommand = new DelegateCommand(OnComboDelete, CanMoveCombo);
            CancelStructureCreationCommand = new DelegateCommand(OnCancelStructure);
            SaveStructureCreationCommand = new DelegateCommand(OnSaveStructure);
            _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Subscribe(OnStructureBuilderChanged);
            //add that very first step.
            if (!String.IsNullOrEmpty(structure?.StructureId))
            {
                NewStructureId = structure.StructureId;
            }
            if (!String.IsNullOrEmpty(structure?.StructureComment))
            {
                BuildStepsFromComment(structure.StructureComment);
            }
            else
            {
                OnAddBase();
            }
        }

        private void BuildStepsFromComment(string comment)
        {
            //use the same service that interprets the comment to interpret this.
            List<StructureGrouping> structureGroups = StructureGenerationService.GetStructureGroupingFromComment(comment);
            int category = 0;
            string operationSaver = String.Empty;
            foreach (var group in structureGroups)//.OrderByDescending(x => x.groupDepth).ThenBy(x => x.groupNum))
            {
                //if (category == 1)
                //{
                //    SelectedStructureOperation = operationSaver;
                //}
                int stepCount = 0;
                switch (category)
                {
                    case 0:
                        stepCount = BaseSteps.Count;
                        break;
                    case 1:
                        stepCount = TargetSteps.Count;
                        SelectedStructureOperation = operationSaver;
                        break;
                    case 2:
                        SelectedComboOperation = operationSaver;
                        stepCount = ComboSteps.Count;
                        break;
                }
              
                //string operationKeep = String.Empty;
                foreach (var structureStep in group.steps)
                {
                    string structureId = StructureModelByString(_plan, structureStep.structureId);
                    SimpleStepModel groupStep = new SimpleStepModel(stepCount, _plan, (SimpleStructureStepSource)stepCount, _eventAggregator);

                    groupStep.SelectedStructure = groupStep.Structures.FirstOrDefault(s => s.StructureId == structureId);
                    if (!String.IsNullOrEmpty(structureStep.structureMargin))
                    {
                        groupStep.Margin = structureStep.structureMargin;
                    }
                    switch (category)
                    {
                        case 0:
                            BaseSteps.Add(groupStep);
                            break;
                        case 1:
                            TargetSteps.Add(groupStep);
                            break;
                        case 2:
                            ComboSteps.Add(groupStep);
                            break;
                    }
                }
                if (ComboSteps.Count > 0)
                {
                    OnGroupSteps();
                }
                operationSaver = group.groupOperation;
                category++;
            }
        }

        

        private void OnSaveStructure()
        {
            if (String.IsNullOrEmpty(NewStructureId))
            {
                MessageBox.Show("Please input a structure Id for the new structure.");
                return;
            }
            (bool bvalidate, string validError) = ValidateSteps();
            if (bvalidate)//first check base and target validation
            {
                //then check combo validations
                string structureComment = String.Empty;
                if (ComboSteps.Any() && !String.IsNullOrEmpty(SelectedComboOperation))
                {
                    string commentStart = "{";
                    structureComment = BuildStructureComment(commentStart);
                    structureComment += "}";
                    if (bGrouped && !String.IsNullOrEmpty(SummaryMargin))
                    {
                        structureComment += $"|{SummaryMargin}";
                    }
                    structureComment += $" {SelectedComboOperation} ";
                    structureComment += "{";
                    foreach (var step in ComboSteps)
                    {
                        structureComment = AppendCommendToStep(structureComment, step, ComboSteps.ToList());
                    }
                    structureComment += "}";
                    if (!String.IsNullOrEmpty(ComboMargin))
                    {
                        structureComment += $"|{ComboMargin}";
                    }
                }
                else
                {
                    //just base and target structures.
                    string commentStart = String.Empty;
                    if (bGrouped && !string.IsNullOrEmpty(SummaryMargin))
                    {
                        commentStart = "{";
                    }
                    structureComment = BuildStructureComment(commentStart);
                    if (bGrouped && !String.IsNullOrEmpty(SummaryMargin))
                    {
                        structureComment += "}";
                        structureComment += $"|{SummaryMargin}";
                    }
                }
                StructureModel newStructure = new StructureModel(_eventAggregator);
                newStructure.StructureId = NewStructureId;
                newStructure.StructureCode = "9999";
                newStructure.TemplateStructureId = NewStructureId;
                newStructure.StructureComment = structureComment;
                //MessageBox.Show(structureComment);
                newStructure.AutoGenerated = true;
                _eventAggregator.GetEvent<AddStructureEvent>().Publish(newStructure);
            }
            else
            {
                MessageBox.Show(validError);
            }
        }

        private string BuildStructureComment(string commentStart)
        {
            string structureComment = commentStart;
            structureComment += "{";
            foreach (var step in BaseSteps)
            {
                structureComment = AppendCommendToStep(structureComment, step, BaseSteps.ToList());
            }
            //sometimes the OR gets stuck on the end of the structure notes. Here we want to remove it.
            if(structureComment.EndsWith(" OR "))
            {
                structureComment.Remove(structureComment.LastIndexOf(" OR "));
            }
            structureComment += "}";
            if (!String.IsNullOrEmpty(BaseMargin))
            {
                structureComment += $"|{BaseMargin}";
            }
            if (TargetSteps.Any() && !String.IsNullOrEmpty(SelectedStructureOperation))
            {
                structureComment += $" {SelectedStructureOperation} ";
                structureComment += "{";
                foreach (var step in TargetSteps)
                {
                    structureComment = AppendCommendToStep(structureComment, step, TargetSteps.ToList());
                }
                structureComment += "}";
                if (!String.IsNullOrEmpty(TargetMargin))
                {
                    structureComment += $"|{TargetMargin}";
                }
            }
            return structureComment;
        }

        private string AppendCommendToStep(string structureComment, SimpleStepModel step, List<SimpleStepModel> steps)
        {
            structureComment += $"<{step.SelectedStructure.StructureId}>";
            if (!String.IsNullOrEmpty(step.Margin))
            {
                structureComment += $"|{step.Margin}";
            }
            if (step.StepId != steps.Count() - 1)
            {
                structureComment += " OR ";
            }

            return structureComment;
        }

        private void OnCancelStructure()
        {
            _eventAggregator.GetEvent<AddStructureEvent>().Publish(null);
        }

        private void OnComboDelete()
        {
            foreach (var step in ComboSteps.Where(bs => bs.StepId > SelectedBaseStep.StepId))
            {
                step.StepId--;
            }
            ComboSteps.Remove(SelectedBaseStep);
        }

        private void OnComboUp()
        {
            if (SelectedComboStep.StepId != 0)
            {
                int currentId = SelectedComboStep.StepId;
                var comboStepsTemp = ComboSteps.ToList();
                comboStepsTemp[currentId - 1].StepId = currentId;
                comboStepsTemp[currentId].StepId = currentId - 1;
                ComboSteps.Clear();
                foreach (var step in comboStepsTemp.OrderBy(cst => cst.StepId))
                {
                    ComboSteps.Add(step);
                }
            }
        }

        private void OnComboDown()
        {
            if (SelectedComboStep.StepId != ComboSteps.Count() - 1)
            {
                int currentId = SelectedComboStep.StepId;
                var comboStepsTemp = ComboSteps.ToList();
                comboStepsTemp[currentId + 1].StepId = currentId;
                comboStepsTemp[currentId].StepId = currentId + 1;
                ComboSteps.Clear();
                foreach (var step in comboStepsTemp.OrderBy(cst => cst.StepId))
                {
                    ComboSteps.Add(step);
                }
            }
        }

        private bool CanMoveCombo()
        {
            return SelectedComboStep != null;
        }

        private void OnAddCombo()
        {
            SimpleStepModel simpleStep = new SimpleStepModel(ComboSteps.Count(), _plan, SimpleStructureStepSource.Combo, _eventAggregator);
            ComboSteps.Add(simpleStep);
        }

        private void OnStructureBuilderChanged(SimpleStepModel obj)
        {
            //do not remove summaries if dealing with the final boolean operators. 
            if (obj.Source != SimpleStructureStepSource.Combo)
            {
                bGrouped = false;
            }
        }

        private void OnGroupSteps()
        {
            (bool bValidation, string validationError) = ValidateSteps();
            if (bValidation)
            {
                bGrouped = true;
                BaseSummary = "(";
                foreach (var step in BaseSteps)
                {
                    BaseSummary += step.SelectedStructure.StructureId;
                    if (!String.IsNullOrEmpty(step.Margin))
                    {
                        BaseSummary += $" + {step.Margin} ";
                    }
                    if (step.StepId != BaseSteps.Max(bs => bs.StepId))
                    {
                        BaseSummary += " OR \n";
                    }
                }
                BaseSummary += ")";
                if (!String.IsNullOrEmpty(BaseMargin)) { BaseSummary += $" + {BaseMargin}"; }
                if (TargetSteps.Any())
                {
                    TargetSummary = "(";
                    foreach (var step in TargetSteps)
                    {
                        TargetSummary += step.SelectedStructure.StructureId;
                        if (!String.IsNullOrEmpty(step.Margin))
                        {
                            TargetSummary += $" + {step.Margin}";
                        }
                        if (step.StepId != TargetSteps.Max(ts => ts.StepId))
                        {
                            TargetSummary += " OR \n";
                        }
                    }
                    TargetSummary += ")";
                    if (!String.IsNullOrEmpty(TargetMargin)) { TargetSummary += $" + {TargetMargin}"; }
                }
            }
            else
            {
                MessageBox.Show(validationError);
            }
        }

        private (bool, string) ValidateSteps()
        {
            if (BaseSteps.Any())
            {
                if (TargetSteps.Any())
                {
                    //check to see that there is an operation between the two.
                    if (String.IsNullOrEmpty(SelectedStructureOperation))
                    {
                        return (false, "No operation between base structure group and target structure group");
                    }
                    else
                    {
                        if (BaseSteps.Any(bs => bs.SelectedStructure == null) || TargetSteps.Any(ts => ts.SelectedStructure == null))
                        {
                            return (false, "Some steps missing assigned structure");
                        }
                        else
                        {
                            return (true, "Valid for base and target structures");
                        }
                    }
                }
                else
                {
                    //check only the base steps.
                    //at a minimum they need a selected structure. 
                    if (BaseSteps.Any(bs => bs.SelectedStructure == null))
                    {
                        return (false, "At least one base step missing assigned structure");
                    }
                    else
                    {
                        return (true, "All base structures selected");
                    }
                }
            }
            else
            {
                return (false, "No Base Steps");
            }
        }

        private void OnTargetDelete()
        {
            foreach (var step in TargetSteps.Where(bs => bs.StepId > SelectedTargetStep.StepId))
            {
                step.StepId--;
            }
            TargetSteps.Remove(SelectedTargetStep);
            bGrouped = false;
        }

        private void OnTargetUp()
        {
            if (SelectedTargetStep.StepId != 0)
            {
                int currentId = SelectedTargetStep.StepId;
                var targetStepsTemp = TargetSteps.ToList();
                targetStepsTemp[currentId - 1].StepId = currentId;
                targetStepsTemp[currentId].StepId = currentId - 1;
                TargetSteps.Clear();
                foreach (var step in targetStepsTemp.OrderBy(tst => tst.StepId))
                {
                    TargetSteps.Add(step);
                }
            }
            bGrouped = false;
        }

        private void OnTargetDown()
        {
            if (SelectedTargetStep.StepId != TargetSteps.Count() - 1)
            {
                int currentId = SelectedTargetStep.StepId;
                var targetStepsTemp = TargetSteps.ToList();
                targetStepsTemp[currentId + 1].StepId = currentId;
                targetStepsTemp[currentId].StepId = currentId + 1;
                TargetSteps.Clear();
                foreach (var step in targetStepsTemp.OrderBy(tst => tst.StepId))
                {
                    TargetSteps.Add(step);
                }
            }
            bGrouped = false;
        }

        private bool CanTargetMove()
        {
            return SelectedTargetStep != null;
        }

        private void OnAddTarget()
        {
            SimpleStepModel simpleStep = new SimpleStepModel(TargetSteps.Count(), _plan, SimpleStructureStepSource.Target, _eventAggregator);
            TargetSteps.Add(simpleStep);
            bGrouped = false;
        }

        private void OnBaseDelete()
        {
            foreach (var step in BaseSteps.Where(bs => bs.StepId > SelectedBaseStep.StepId))
            {
                step.StepId--;
            }
            BaseSteps.Remove(SelectedBaseStep);
            bGrouped = false;
        }

        private void OnBaseUp()
        {
            if (SelectedBaseStep.StepId != 0)
            {
                int currentId = SelectedBaseStep.StepId;
                var baseStepsTemp = BaseSteps.ToList();
                baseStepsTemp[currentId - 1].StepId = currentId;
                baseStepsTemp[currentId].StepId = currentId - 1;
                BaseSteps.Clear();
                foreach (var step in baseStepsTemp.OrderBy(bst => bst.StepId))
                {
                    BaseSteps.Add(step);
                }
            }
            bGrouped = false;
        }

        private bool CanBaseMove()
        {
            return SelectedBaseStep != null;

        }

        private void OnBaseDown()
        {
            if (SelectedBaseStep.StepId != BaseSteps.Count() - 1)
            {
                int currentId = SelectedBaseStep.StepId;
                var baseStepsTemp = BaseSteps.ToList();
                baseStepsTemp[currentId + 1].StepId = currentId;
                baseStepsTemp[currentId].StepId = currentId + 1;
                BaseSteps.Clear();
                foreach (var step in baseStepsTemp.OrderBy(bst => bst.StepId))
                {
                    BaseSteps.Add(step);
                }
            }
            bGrouped = false;
        }

        private void OnAddBase()
        {
            SimpleStepModel simpleStep = new SimpleStepModel(BaseSteps.Count(), _plan, SimpleStructureStepSource.Base, _eventAggregator);
            BaseSteps.Add(simpleStep);
            bGrouped = false;
        }

        private void UpdateOperationImage()
        {
            switch (SelectedStructureOperation)
            {
                case "AND":
                    OperationImageSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "AndBoolean.PNG");
                    break;
                case "OR":
                    OperationImageSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "OrBoolean.PNG");
                    break;
                case "SUB":
                    OperationImageSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "SubBoolean.PNG");
                    break;
                default:
                    OperationImageSource = String.Empty;
                    break;
            }
            bGrouped = false;
        }
        private void UpdateComboImage()
        {
            switch (SelectedComboOperation)
            {
                case "AND":
                    ComboImageSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "AndBoolean.PNG");
                    break;
                case "OR":
                    ComboImageSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "OrBoolean.PNG");
                    break;
                case "SUB":
                    ComboImageSource = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "SubBoolean.PNG");
                    break;
                default:
                    ComboImageSource = String.Empty;
                    break;
            }
        }

    }
}
