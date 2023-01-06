using PlanScoreCard.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels.VMHelpers
{
    public class StructureMatchingViewModel : BindableBase
    {
        public StructureModel CurrentStructure { get; }
        public ObservableCollection<StructureModel> PlanStructures { get; set; }
        private StructureModel _selectedPlanStructure;

        public StructureModel SelectedPlanStructure
        {
            get { return _selectedPlanStructure; }
            set 
            { 
                SetProperty(ref _selectedPlanStructure, value);
                MatchStructureCommand.RaiseCanExecuteChanged();
            }
        }
        public DelegateCommand MatchStructureCommand { get; private set; }
        public StructureMatchingViewModel(StructureModel structure, List<StructureModel> planStructures)
        {
            CurrentStructure = structure;
            MatchStructureCommand = new DelegateCommand(OnMatchStructure, CanMatchStructure);

            PlanStructures = new ObservableCollection<StructureModel>();
            foreach(var ps in planStructures)
            {
                PlanStructures.Add(ps);
            }
            if(structure.MatchedStructure != null)
            {
                SelectedPlanStructure = structure.MatchedStructure;
            }
        }

        private void OnMatchStructure()
        {
            CurrentStructure.MatchedStructure = SelectedPlanStructure;
            //CurrentStructure.bValidStructure = true;
            //CurrentStructure.bStructureMatch = true;
            //CurrentStructure.bLocalMatch = true;
            OnClose();
        }

        private void OnClose()
        {
            CurrentStructure.OnCloseStructureMatch();
        }

        private bool CanMatchStructure()
        {
            return SelectedPlanStructure != null;
        }
    }
}
