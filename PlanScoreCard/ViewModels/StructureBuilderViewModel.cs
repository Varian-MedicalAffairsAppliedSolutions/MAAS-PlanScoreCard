using PlanScoreCard.Events;
using PlanScoreCard.Models;
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

namespace PlanScoreCard.ViewModels
{
    public class StructureBuilderViewModel : BindableBase
    {
        private PlanModel _planModel;
        private IEventAggregator _eventAggregator;
        private string _structureIdBuilder;
        public string StructureIdBuilder
        {

            get { return _structureIdBuilder; }
            set { SetProperty(ref _structureIdBuilder, value); }
        }
        private string _structureCommentBuilder;

        public string StructureCommentBuilder
        {
            get { return _structureCommentBuilder; }
            set { SetProperty(ref _structureCommentBuilder, value); }
        }
        private StructureModel _selectedBaseStructure;

        public StructureModel SelectedBaseStructure
        {
            get { return _selectedBaseStructure; }
            set
            {
                SetProperty(ref _selectedBaseStructure, value);
                //AddStructureCommand.RaiseCanExecuteChanged();
                IncreaseMarginCommand.RaiseCanExecuteChanged();
                DecreaseMarginCommand.RaiseCanExecuteChanged();
                if (SelectedBaseStructure != null)
                {
                    _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(null);
                }
            }
        }
        private int _structureMargin;

        public int StructureMargin
        {
            get { return _structureMargin; }
            set
            {
                SetProperty(ref _structureMargin, value);
                //OnStructureBuilderChanged(null);
                _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(null);
            }
        }
        public DelegateCommand<Window> CancelStructureCommand { get; private set; }
        public DelegateCommand<Window> AddStructureCommand { get; private set; }
        public DelegateCommand IncreaseMarginCommand { get; private set; }
        public DelegateCommand DecreaseMarginCommand { get; private set; }
        public StructureBuilderGroupViewModel StructureGroup1 { get; private set; }
        public StructureBuilderGroupViewModel StructureGroup2 { get; private set; }
        public ObservableCollection<OperationModel> Operations { get; private set; }

        private OperationModel _selectedOperation;

        public OperationModel SelectedOperation
        {
            get { return _selectedOperation; }
            set
            {
                SetProperty(ref _selectedOperation, value);
                if (SelectedOperation != null)
                {
                    OnStructureBuilderChanged(null);
                    StructureGroup1.StructureOperations.First().SelectedOperation = SelectedOperation;
                }
            }
        }
        public ObservableCollection<OperationModel> NestingOperations { get; private set; }
        private OperationModel _selectedNestingOperation;

        public OperationModel SelectedNestingOperation
        {
            get { return _selectedNestingOperation; }
            set
            {
                _selectedNestingOperation = value;
                OnStructureBuilderChanged(null);
                StructureGroup2.StructureOperations.First().SelectedOperation = SelectedNestingOperation;
            }
        }


        private bool _bnesting;

        public bool bNesting
        {
            get { return _bnesting; }
            set { SetProperty(ref _bnesting, value); }
        }
        private int _nestingColumn;

        public int NestingColumn
        {
            get { return _nestingColumn; }
            set { SetProperty(ref _nestingColumn, value); }
        }
        private int _baseColumn;

        public int BaseColumn
        {
            get { return _baseColumn; }
            set { SetProperty(ref _baseColumn, value); }
        }
        private int _nestingOperationColumn;

        public int NestingOperationColumn
        {
            get { return _nestingOperationColumn; }
            set { SetProperty(ref _nestingOperationColumn, value); }
        }

        private string _commentBase;

        public string CommentBase
        {
            get { return _commentBase; }
            set { SetProperty(ref _commentBase, value); }
        }
        public ObservableCollection<StructureModel> Structures { get; private set; }

        public StructureBuilderViewModel(PlanModel planSetup, IEventAggregator eventAggregator)
        {
            _planModel = planSetup;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Subscribe(OnStructureBuilderChanged);
            CancelStructureCommand = new DelegateCommand<Window>(OnCancelStructure);
            AddStructureCommand = new DelegateCommand<Window>(OnAddStructure, CanAddStructure);
            StructureGroup1 = new StructureBuilderGroupViewModel(StructureModelNestingModel.None,
                _planModel, _eventAggregator);
            StructureGroup1.bIsNestable = true;
            StructureGroup2 = new StructureBuilderGroupViewModel(StructureModelNestingModel.None,
                _planModel, _eventAggregator);
            StructureGroup2.bIsNestable = false;
            StructureGroup2.bIsVisible = false;
            _eventAggregator.GetEvent<ChangeNestEvent>().Subscribe(OnNestChanged);
            Operations = new ObservableCollection<OperationModel>();
            NestingOperations = new ObservableCollection<OperationModel>();
            Operations.Add(new OperationModel("AND", StructureOperationEnum.AND, "&"));
            Operations.Add(new OperationModel("OR", StructureOperationEnum.OR, "U"));
            Operations.Add(new OperationModel("SUB", StructureOperationEnum.SUB, "#"));
            Operations.Add(new OperationModel("RING", StructureOperationEnum.RING, "O"));
            NestingOperations.Add(new OperationModel("AND", StructureOperationEnum.AND, "&"));
            NestingOperations.Add(new OperationModel("OR", StructureOperationEnum.OR, "U"));
            NestingOperations.Add(new OperationModel("SUB", StructureOperationEnum.SUB, "#"));

            Structures = new ObservableCollection<StructureModel>();
            foreach (var structure in _planModel.Structures)
            {
                Structures.Add(structure);
            }
            IncreaseMarginCommand = new DelegateCommand(OnIncreaseMargin, CanIncreaseMargin);
            DecreaseMarginCommand = new DelegateCommand(OnDecreaseMargin, CanIncreaseMargin);
            //set column defaults.
            NestingColumn = 0;
            NestingOperationColumn = 2;
            BaseColumn = 3;
        }

        private void OnNestChanged()
        {
            //determine nesting.
            var nesting = StructureGroup1.Nesting;
            //modify nesting of structuregroup2.
            if (nesting == StructureModelNestingModel.NestLeft)
            {
                BaseColumn = 0;
                NestingOperationColumn = 1;
                NestingColumn = 2;
            }
            else if (nesting == StructureModelNestingModel.NestRight)
            {
                BaseColumn = 3;
                NestingOperationColumn = 2;
                NestingColumn = 0;
            }
            StructureGroup2.bIsVisible = true;
            bNesting = true;
            OnStructureBuilderChanged(null);
        }

        private void OnAddStructure(Window obj)
        {
            var structure = new StructureModel
            {
                StructureId = StructureIdBuilder,
                StructureCode = "9999",
                StructureComment = StructureCommentBuilder,
                AutoGenerated = true
            };
            //TODO make this a different event because planchanged event is used in other places.
            //_eventAggregator.GetEvent<PlanChangedEvent>().Publish(null);
            _eventAggregator.GetEvent<AddStructureEvent>().Publish(structure);
            obj.Close();
        }

        private bool CanAddStructure(Window arg)
        {
            //todo determine when the structure add can be selected.
            if (StructureGroup1.Nesting == StructureModelNestingModel.None)
            {
                return SelectedBaseStructure != null;
            }
            else
            {
                return (SelectedBaseStructure != null &&
                    SelectedBaseStructure != null &&
                    SelectedOperation != null);
            }
        }

        private void OnCancelStructure(Window obj)
        {
            obj.Close();
        }

        private void OnStructureBuilderChanged(StructureOperationModel obj)
        {
            //build structure IDs.
            if (SelectedBaseStructure != null)
            {
                if (StructureGroup1.Nesting != StructureModelNestingModel.None)
                {
                    //there will be nesting here.
                    var prefix = ConfigurationManager.AppSettings["AutoStructurePrefix"];
                    StructureCommentBuilder = String.Empty;

                    if (StructureGroup1.Nesting == StructureModelNestingModel.NestLeft)
                    {

                        StructureCommentBuilder = $"({SelectedBaseStructure.StructureId}";

                        if (StructureMargin != 0)
                        {
                            StructureCommentBuilder += $"|{StructureMargin}";
                        }
                        BuildStructureComment(StructureGroup1, false, false);
                        StructureCommentBuilder += ")";
                        if (StructureGroup1.StructureMargin != 0.0)
                        {
                            StructureCommentBuilder += $"|{StructureGroup1.StructureMargin}";
                        }
                        if (SelectedNestingOperation != null)
                        {
                            StructureCommentBuilder += $" {SelectedNestingOperation.SelectedOperationTxt} ";
                        }
                        StructureCommentBuilder += " (";
                        BuildStructureComment(StructureGroup2, true, false);
                        StructureCommentBuilder += ")";
                        if (StructureGroup2.StructureMargin != 0.0)
                        {
                            StructureCommentBuilder += $"|{StructureGroup2.StructureMargin} ";
                        }
                        CommentBase = StructureCommentBuilder.Split(')').First().TrimStart('(');
                        if (SelectedBaseStructure != null && SelectedNestingOperation != null)
                        {
                            StructureIdBuilder = String.Empty;
                            StructureIdBuilder = $"{prefix}{SelectedBaseStructure.StructureId}{SelectedNestingOperation.SelectedOperationChar}";
                            if (StructureGroup2.StructureOperations != null && StructureGroup2.StructureOperations.FirstOrDefault().SelectedStructure != null)
                            {
                                StructureIdBuilder += $"{StructureGroup2.StructureOperations.FirstOrDefault().SelectedStructure.StructureId}";
                            }
                        }
                    }
                    else
                    {
                        StructureCommentBuilder = "(";
                        BuildStructureComment(StructureGroup2, true, false);
                        StructureCommentBuilder += ")";
                        if (StructureGroup2.StructureMargin != 0.0)
                        {
                            StructureCommentBuilder += $"|{StructureGroup2.StructureMargin}";
                        }
                        if (SelectedNestingOperation != null)
                        {
                            StructureCommentBuilder += $" {SelectedNestingOperation.SelectedOperationTxt} ";
                        }
                        StructureCommentBuilder = $"({SelectedBaseStructure.StructureId}";

                        if (StructureMargin != 0)
                        {
                            StructureCommentBuilder += $"|{StructureMargin}";
                        }
                        BuildStructureComment(StructureGroup1, false, false);
                        StructureCommentBuilder += ")";
                        if (StructureGroup1.StructureMargin != 0.0)
                        {
                            StructureCommentBuilder += $"|{StructureGroup1.StructureMargin}";
                        }
                        CommentBase = StructureCommentBuilder.Split('(').Last().Split(')').First();
                        if (StructureGroup2.StructureOperations != null && StructureGroup2.StructureOperations.First().SelectedStructure != null && SelectedNestingOperation != null)
                        {
                            StructureIdBuilder = String.Empty;
                            StructureIdBuilder = $"{prefix}{StructureGroup2.StructureOperations.First().SelectedStructure.StructureId}{SelectedNestingOperation.SelectedOperationChar}";
                            if (SelectedBaseStructure != null)
                            {
                                StructureIdBuilder += $"{SelectedBaseStructure.StructureId}";
                            }
                        }
                    }
                }
                else//no nesting, only one structure group.
                {

                    var prefix = ConfigurationManager.AppSettings["AutoStructurePrefix"];
                    StructureIdBuilder = String.Empty;
                    StructureIdBuilder += $"{prefix}{SelectedBaseStructure.StructureId}";

                    StructureCommentBuilder = String.Empty;
                    StructureCommentBuilder = $"{SelectedBaseStructure.StructureId}";
                    if (StructureMargin != 0)
                    {
                        StructureIdBuilder += $"|{StructureMargin}";
                        StructureCommentBuilder += $"|{StructureMargin}";
                    }
                    BuildStructureComment(StructureGroup1, false, true);



                    //set base comment
                    CommentBase = StructureCommentBuilder;
                }
                if (StructureIdBuilder.Length > 13)
                {
                    StructureIdBuilder = StructureIdBuilder.Substring(0, 13);
                }
            }
            AddStructureCommand.RaiseCanExecuteChanged();
        }

        private void BuildStructureComment(StructureBuilderGroupViewModel structureGroup, bool skipOperation1, bool idBuiler)
        {
            int i = 0;
            foreach (var structure_obj in structureGroup.StructureOperations)
            {
                if (i == 0 && skipOperation1)
                {
                    StructureGroup1.bMarginVis = true;
                    if (structure_obj.SelectedStructure != null)
                    {
                        StructureCommentBuilder += structure_obj.SelectedStructure.StructureId;
                    }

                    if (structure_obj.StructureMargin != 0)
                    {
                        StructureCommentBuilder += $"|{structure_obj.StructureMargin}"; // structure_obj.Margin > 0 ? $"+{structure_obj.Margin}mm" : structure_obj.Margin.ToString() + "mm";
                    }
                    i++;
                }
                else
                {
                    if (structure_obj.SelectedOperation != null)
                    {
                        StructureCommentBuilder += " " + structure_obj.SelectedOperation.SelectedOperationTxt + " ";
                        StructureIdBuilder += structure_obj.SelectedOperation.SelectedOperationChar;
                        if (structure_obj.SelectedOperation.SelectedOperationEnum == StructureOperationEnum.RING)
                        {
                            StructureGroup1.bMarginVis = false;
                            if (StructureMargin != 0)
                            {
                                //StructureIdBuilder = StructureIdBuilder.Replace($"|{StructureMargin}", "");
                                StructureCommentBuilder = StructureCommentBuilder.Replace($"|{StructureMargin}", "");
                            }
                            //StructureGroup1.StructureMargin = 0;
                            StructureCommentBuilder += $"{structure_obj.InnerMargin}*{structure_obj.OuterMargin}"; //$"{(structure_obj.InnerMargin > 0 ? $"+{structure_obj.InnerMargin}mm" : $"{structure_obj.InnerMargin}mm")}|{(structure_obj.OuterMargin > 0 ? $"+{structure_obj.OuterMargin}mm" : $"{structure_obj.OuterMargin}mm")}";
                        }
                        else
                        {
                            StructureGroup1.bMarginVis = true;
                            if (structure_obj.SelectedStructure != null)
                            {
                                StructureCommentBuilder += structure_obj.SelectedStructure.StructureId;
                                StructureIdBuilder += structure_obj.SelectedStructure.StructureId;
                            }
                        }
                        if (structure_obj.StructureMargin != 0)
                        {
                            StructureCommentBuilder += $"|{structure_obj.StructureMargin}"; // structure_obj.Margin > 0 ? $"+{structure_obj.Margin}mm" : structure_obj.Margin.ToString() + "mm";
                        }
                    }
                }

            }
        }

        private void OnIncreaseMargin()
        {
            StructureMargin++;
        }

        private void OnDecreaseMargin()
        {
            StructureMargin--;
        }

        private bool CanIncreaseMargin()
        {
            return SelectedBaseStructure != null;
        }
    }
}

