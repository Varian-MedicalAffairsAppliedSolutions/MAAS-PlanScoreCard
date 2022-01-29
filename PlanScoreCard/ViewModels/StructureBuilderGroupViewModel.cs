using PlanScoreCard.Events;
using PlanScoreCard.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlanScoreCard.ViewModels
{
    public class StructureBuilderGroupViewModel : BindableBase
    {
        private PlanModel _planModel;
        private IEventAggregator _eventAggregator;



        private StructureModelNestingModel _nesting;

        public StructureModelNestingModel Nesting
        {
            get { return _nesting; }
            set { SetProperty(ref _nesting, value); }
        }
        private bool _bisVisible;

        public bool bIsVisible
        {
            get { return _bisVisible; }
            set { SetProperty(ref _bisVisible, value); }
        }
        private bool _bisNestable;

        public bool bIsNestable
        {
            get { return _bisNestable; }
            set { SetProperty(ref _bisNestable, value); }
        }
        private bool _bmarginVis;

        public bool bMarginVis
        {
            get { return _bmarginVis; }
            set { SetProperty(ref _bmarginVis, value); }
        }
        private int _structureMargin;

        public int StructureMargin
        {
            get { return _structureMargin; }
            set
            {
                SetProperty(ref _structureMargin, value);
                _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(null);
            }
        }
        public DelegateCommand IncreaseMarginCommand { get; private set; }
        public DelegateCommand DecreaseMarginCommand { get; private set; }
        // public ObservableCollection<StructureModel> Structures { get; set; }
        public ObservableCollection<StructureOperationModel> StructureOperations { get; set; }
        public DelegateCommand AddOperationCommand { get; private set; }
        public DelegateCommand NestLeftCommand { get; private set; }
        public DelegateCommand NestRightCommand { get; private set; }
        public StructureBuilderGroupViewModel(StructureModelNestingModel nesting, PlanModel planModel,
            IEventAggregator eventAggregator)
        {
            Nesting = nesting;
            _planModel = planModel;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<DeleteStructureOperationEvent>().Subscribe(OnDeleteStructureOperation);
            bMarginVis = true;
            IncreaseMarginCommand = new DelegateCommand(OnIncreaseMargin, CanIncreaseMargin);
            DecreaseMarginCommand = new DelegateCommand(OnDecreaseMargin, CanIncreaseMargin);
            NestLeftCommand = new DelegateCommand(OnNestLeft);
            NestRightCommand = new DelegateCommand(OnNestRight);
            AddOperationCommand = new DelegateCommand(OnAddOperation);
            StructureOperations = new ObservableCollection<StructureOperationModel>();
            AddStructureOperationModel();
        }

        private void OnDeleteStructureOperation(StructureOperationModel obj)
        {
            if (StructureOperations.Count() > 1 && obj != StructureOperations.First())
            {
                StructureOperations.Remove(obj);
                _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(null);
            }
        }

        private void OnNestLeft()
        {
            Nesting = StructureModelNestingModel.NestLeft;
            _eventAggregator.GetEvent<ChangeNestEvent>().Publish();
        }

        private void OnNestRight()
        {
            Nesting = StructureModelNestingModel.NestRight;
            _eventAggregator.GetEvent<ChangeNestEvent>().Publish();
        }
        //commented margins on the base structure (moved to StructureBuilderViewModel MCS 4.18.21)
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
            return true;
        }
        private void OnAddOperation()
        {
            if (StructureOperations.Last().SelectedOperation != null && StructureOperations.Last().SelectedStructure != null && StructureOperations.Last().SelectedOperation.SelectedOperationEnum != StructureOperationEnum.RING)
            {
                AddStructureOperationModel();
            }
        }

        public void AddStructureOperationModel()
        {
            StructureOperationModel structureOperationModel = new StructureOperationModel(_eventAggregator);
            foreach (var structure in _planModel.Structures)
            {
                structureOperationModel.Structures.Add(structure);
            }
            //only OR operation for subsequent structure additions.
            structureOperationModel.Operations.Add(new OperationModel("AND", StructureOperationEnum.AND, "&"));
            structureOperationModel.Operations.Add(new OperationModel("OR", StructureOperationEnum.OR, "U"));
            structureOperationModel.Operations.Add(new OperationModel("SUB", StructureOperationEnum.SUB, "#"));
            if (StructureOperations.Count() > 0)
            {
                structureOperationModel.Operations.Add(new OperationModel("RING", StructureOperationEnum.RING, "O"));
                structureOperationModel.SelectedOperation = structureOperationModel.Operations.FirstOrDefault(x => x.SelectedOperationEnum == StructureOperationEnum.OR);//structureOperationModel.Operations.FirstOrDefault(x => x.SelectedOperationEnum == StructureOperations.FirstOrDefault().SelectedOperation.SelectedOperationEnum);
                structureOperationModel.bOperationVis = false;
                structureOperationModel.OperationTxt = "OR";
            }
            else
            {
                //rings are stand-alone and cannot be combined with other operations.
                structureOperationModel.OperationTxt = "";
            }
            StructureOperations.Add(structureOperationModel);
        }
    }
}
