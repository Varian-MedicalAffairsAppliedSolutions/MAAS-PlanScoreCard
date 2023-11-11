using PlanScoreCard.ViewModels.VMHelpers;
using PlanScoreCard.Views.HelperWindows;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class SimpleStepModel:BindableBase
    {
        public List<StructureModel> Structures { get; private set; }

        private PlanModel _plan;
        private IEventAggregator _eventAggregator;

        private StructureModel _selectedStructure;

        public StructureModel SelectedStructure
        {
            get { return _selectedStructure; }
            set { SetProperty(ref _selectedStructure, value); }
        }

        private string _margin;

        public string Margin
        {
            get { return _margin; }
            set { SetProperty(ref _margin,value); }
        }
        public int StepId { get; set; }
        public DelegateCommand AssymetricMarginCommand { get; private set; }
        private AsymmetricMarginView _asymmetricMarginView { get; set; }
        public SimpleStepModel(int stepId, PlanModel plan, IEventAggregator eventAggregator)
        {
            StepId = stepId;
            _plan = plan;
            _eventAggregator = eventAggregator;

            Structures = new List<StructureModel>();

            AssymetricMarginCommand = new DelegateCommand(OnAssymetricMargin);
            AddStructures();
        }
        private void AddStructures()
        {
            foreach (var s in _plan.Structures)
            {
                Structures.Add(s);
            }
        }
        private void OnAssymetricMargin()
        {
            if (_asymmetricMarginView == null)
            {
                _asymmetricMarginView = new AsymmetricMarginView();
            }
            _asymmetricMarginView.DataContext = new AsymmetricMarginViewModel(Margin, _eventAggregator);
            _asymmetricMarginView.ShowDialog();
        }
    }
}
