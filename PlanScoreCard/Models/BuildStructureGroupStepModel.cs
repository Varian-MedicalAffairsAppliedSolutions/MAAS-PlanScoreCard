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
            }
        }
        private int _structureMargin;

        public int StructureMargin
        {
            get { return _structureMargin; }
            set { SetProperty(ref _structureMargin, value); }
        }
        private string _selectedOperation;

        public string SelectedOperation
        {
            get { return _selectedOperation; }
            set 
            {
                SetProperty(ref _selectedOperation, value);
                AddGroupStepCommand.RaiseCanExecuteChanged();
            }
        }
        public int StepNumber { get; set; }
        //commands
        public DelegateCommand DecreaseMarginCommand { get; private set; }
        public DelegateCommand IncreaseMarginCommand { get; private set; }
        public DelegateCommand AddGroupStepCommand { get; private set; }
        public DelegateCommand DeleteGroupStepCommand { get; private set; }
        public BuildStructureGroupStepModel(PlanModel plan, IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            Structures = new ObservableCollection<StructureModel>();
            Operations = new List<string> { "AND", "OR", "SUB" };
            AddStructures();
            //commands
            DecreaseMarginCommand = new DelegateCommand(OnDecreaseMargin);
            IncreaseMarginCommand = new DelegateCommand(OnIncreaseMargin);
            AddGroupStepCommand = new DelegateCommand(OnAddGroupStep, CanAddGroupStep);
            DeleteGroupStepCommand = new DelegateCommand(OnDeleteGroupStep);
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
            throw new NotImplementedException();
        }

        private void OnAddGroupStep()
        {
            throw new NotImplementedException();
        }

        private void OnIncreaseMargin()
        {
            StructureMargin++;
        }

        private void OnDecreaseMargin()
        {
            StructureMargin--;
        }

        private void AddStructures()
        {
            foreach (var s in _plan.Structures)
            {
                Structures.Add(s);
            }
        }
    }
}