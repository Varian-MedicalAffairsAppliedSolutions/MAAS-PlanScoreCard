using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Events.StructureBuilder;
using PlanScoreCard.ViewModels.VMHelpers;
using PlanScoreCard.Views.HelperWindows;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PlanScoreCard.Models
{
    public class BuildStructureGroupStepModel : BindableBase
    {
        private PlanModel _plan;
        private IEventAggregator _eventAggregator;

        public ObservableCollection<StructureModel> Structures { get; set; }
        public List<string> Operations { get; set; }
        private StructureModel _selectedStructure;

        public StructureModel SelectedStructure
        {
            get { return _selectedStructure; }
            set 
            {
                SetProperty(ref _selectedStructure, value);
                AddGroupStepCommand.RaiseCanExecuteChanged();
                //IncreaseMarginCommand.RaiseCanExecuteChanged();
                //DecreaseMarginCommand.RaiseCanExecuteChanged();
                AsymmetricMarginCommand.RaiseCanExecuteChanged();
                _eventAggregator.GetEvent<UpdateGroupCommentEvent>().Publish();
            }
        }
        private string _structureMargin;

        public string StructureMargin
        {
            get { return _structureMargin; }
            set { 
                SetProperty(ref _structureMargin, value);
                _eventAggregator.GetEvent<UpdateGroupCommentEvent>().Publish();
            }

        }
        private string _selectedOperation;

        public string SelectedOperation
        {
            get { return _selectedOperation; }
            set 
            {
                SetProperty(ref _selectedOperation, value);
                AddGroupStepCommand.RaiseCanExecuteChanged();
                _eventAggregator.GetEvent<UpdateGroupCommentEvent>().Publish();
            }
        }
        public int StepNumber { get; set; }
        private AsymmetricMarginView _asymmetricMarginView { get; set; }
        //commands
        public DelegateCommand DecreaseMarginCommand { get; private set; }
        public DelegateCommand IncreaseMarginCommand { get; private set; }
        public DelegateCommand AddGroupStepCommand { get; private set; }
        public DelegateCommand DeleteGroupStepCommand { get; private set; }
        public DelegateCommand AsymmetricMarginCommand { get; private set; }
        public BuildStructureGroupStepModel(PlanModel plan, IEventAggregator eventAggregator, int stepNum)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            StepNumber = stepNum;
            Structures = new ObservableCollection<StructureModel>();
            Operations = new List<string> { "AND", "OR", "SUB" };
            AddStructures();
            //commands
            //DecreaseMarginCommand = new DelegateCommand(OnDecreaseMargin, CanMargin);
            //IncreaseMarginCommand = new DelegateCommand(OnIncreaseMargin, CanMargin);
            AsymmetricMarginCommand = new DelegateCommand(OnAsymmetricMargin, CanMargin);
            AddGroupStepCommand = new DelegateCommand(OnAddGroupStep, CanAddGroupStep);
            DeleteGroupStepCommand = new DelegateCommand(OnDeleteGroupStep);
            _eventAggregator.GetEvent<SaveAsymmetricMarginEvent>().Subscribe(OnUpdateMargin);
        }

        private void OnUpdateMargin(AsymmetricMarginViewModel obj)
        {
            if (obj.bSave)
            {
                StructureMargin = $"{obj.LeftMargin}^{obj.RightMargin}^{obj.SupMargin}^{obj.InfMargin}^{obj.PostMargin}^{obj.AntMargin}";
            }
            _asymmetricMarginView.Close();
        }

        private void OnAsymmetricMargin()
        {
            if(_asymmetricMarginView == null)
            {
                _asymmetricMarginView = new AsymmetricMarginView();
            }
            _asymmetricMarginView.DataContext = new AsymmetricMarginViewModel(StructureMargin, _eventAggregator);
            _asymmetricMarginView.ShowDialog();
        }

        private bool CanMargin()
        {
            return SelectedStructure != null;
        }

        private bool CanAddGroupStep()
        {
            if(StepNumber == 0)
            {
                return SelectedStructure != null;
            }
            return SelectedOperation != null && SelectedStructure != null;
        }

        private void OnDeleteGroupStep()
        {
            _eventAggregator.GetEvent<DeleteGroupStepEvent>().Publish(this);
        }

        private void OnAddGroupStep()
        {
            _eventAggregator.GetEvent<AddGroupStepEvent>().Publish();
        }

        //private void OnIncreaseMargin()
        //{
        //    StructureMargin++;
        //}

        //private void OnDecreaseMargin()
        //{
        //    StructureMargin--;
        //}

        private void AddStructures()
        {
            foreach (var s in _plan.Structures)
            {
                Structures.Add(s);
            }
        }
    }
}