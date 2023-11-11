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

namespace PlanScoreCard.ViewModels
{
    public class SimpleStructureBuilderViewModel:BindableBase
    {
        private SimpleStepModel _selectedBaseStep;
        private PlanModel _plan;
        private IEventAggregator _eventAggregator;

        public SimpleStepModel SelectedBaseStep
        {
            get { return _selectedBaseStep; }
            set 
            { 
                SetProperty(ref _selectedBaseStep,value);
                //AddBaseCommand.RaiseCanExecuteChanged();
                BaseUpCommand.RaiseCanExecuteChanged();
                BaseDownCommand.RaiseCanExecuteChanged();
                BaseDeleteCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<SimpleStepModel> BaseSteps { get; private set; }
        //Commands
        public DelegateCommand AddBaseCommand { get; private set; }
        public DelegateCommand BaseDownCommand { get; private set; }
        public DelegateCommand BaseUpCommand { get; private set; }
        public DelegateCommand BaseDeleteCommand { get; private set; }
        public SimpleStructureBuilderViewModel(string comment,PlanModel plan, IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            BaseSteps = new ObservableCollection<SimpleStepModel>();
            AddBaseCommand = new DelegateCommand(OnAddBase);
            BaseDownCommand = new DelegateCommand(OnBaseDown, CanBaseMove);
            BaseUpCommand = new DelegateCommand(OnBaseUp, CanBaseMove);
            BaseDeleteCommand = new DelegateCommand(OnBaseDelete, CanBaseMove);
            //add that very first step.
            OnAddBase();
        }

        private void OnBaseDelete()
        {
            foreach(var step in BaseSteps.Where(bs => bs.StepId > SelectedBaseStep.StepId))
            {
                step.StepId--;
            }
            BaseSteps.Remove(SelectedBaseStep);
        }

        private void OnBaseUp()
        {
            //TODO: Implement
        }

        private bool CanBaseMove()
        {
            return SelectedBaseStep != null;
        }

        private void OnBaseDown()
        {
            //TODO: Implement
        }

        private void OnAddBase()
        {
            SimpleStepModel simpleStep = new SimpleStepModel(BaseSteps.Count(),_plan,_eventAggregator);
            BaseSteps.Add(simpleStep);
        }
    }
}
