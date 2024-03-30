using PlanScoreCard.Events;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models.ModelHelpers;
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
        public SimpleStructureStepSource Source;
        private StructureModel _selectedStructure;

        public StructureModel SelectedStructure
        {
            get { return _selectedStructure; }
            set 
            { 
                SetProperty(ref _selectedStructure, value);
                _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(this);
            }

        }

        private string _margin;

        public string Margin
        {
            get { return _margin; }
            set 
            { 
                SetProperty(ref _margin,value);
                _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(this);
            }
        }
        public int StepId { get; set; }
        public DelegateCommand AssymetricMarginCommand { get; private set; }
        private AsymmetricMarginView _asymmetricMarginView { get; set; }
        public SimpleStepModel(int stepId, PlanModel plan, SimpleStructureStepSource source, IEventAggregator eventAggregator)
        {
            StepId = stepId;
            _plan = plan;
            _eventAggregator = eventAggregator;
            Source = source;

            Structures = new List<StructureModel>();

            AssymetricMarginCommand = new DelegateCommand(OnAssymetricMargin);
            _eventAggregator.GetEvent<SaveAsymmetricMarginEvent>().Subscribe(OnSaveAsymmetricMargin);
            AddStructures();
        }

        private void OnSaveAsymmetricMargin(AsymmetricMarginViewModel obj)
        {
            //this method comes from an event which will run on all subscribers, crashing the application if called on the incorrect model.
            if (obj.Source == Source && obj.StepId == StepId)
            {
                if (obj.bSave)
                {
                    Margin = $"{obj.LeftMargin}^{obj.RightMargin}^{obj.SupMargin}^{obj.InfMargin}^{obj.PostMargin}^{obj.AntMargin}";
                }
                _asymmetricMarginView.Close();
                _asymmetricMarginView = null;
            }
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
                _asymmetricMarginView.DataContext = new AsymmetricMarginViewModel(Margin,Source, StepId, _eventAggregator);
                _asymmetricMarginView.ShowDialog();
            
            
        }
    }
}
