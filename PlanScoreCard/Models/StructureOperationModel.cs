using PlanScoreCard.Events;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace PlanScoreCard.Models
{
    public class StructureOperationModel : BindableBase
    {
        private IEventAggregator _eventAggregator;

        public ObservableCollection<OperationModel> Operations { get; set; }
        private OperationModel _selectedOperation;

        public OperationModel SelectedOperation
        {
            get { return _selectedOperation; }
            set
            {
                SetProperty(ref _selectedOperation, value);
                if (SelectedOperation != null)
                {
                    if (SelectedOperation.SelectedOperationEnum == StructureOperationEnum.RING)
                    {
                        bRing = true;
                        //bMargin = false;
                    }
                    else
                    {
                        bTargetStructure = true;
                    }
                }
                if (SelectedOperation != null)
                {
                    _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(this);
                }
            }
        }
        public ObservableCollection<StructureModel> Structures { get; set; }
        private StructureModel _selectedStructure;

        public StructureModel SelectedStructure
        {
            get { return _selectedStructure; }
            set
            {
                SetProperty(ref _selectedStructure, value);
                if (SelectedOperation != null && SelectedStructure != null)
                {
                    _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(this);
                }
                IncreaseMarginCommand.RaiseCanExecuteChanged();
                DecreaseMarginCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _bTargetStructure;

        public bool bTargetStructure
        {
            get { return _bTargetStructure; }
            set
            {
                SetProperty(ref _bTargetStructure, value);
                if (bTargetStructure)
                {
                    bRing = false;
                }
            }
        }
        private bool _boperationVis;

        public bool bOperationVis
        {
            get { return _boperationVis; }
            set { SetProperty(ref _boperationVis, value); }
        }
        private string _operationTxt;

        public string OperationTxt
        {
            get { return _operationTxt; }
            set { SetProperty(ref _operationTxt, value); }
        }

        private bool _bRing;

        public bool bRing
        {
            get { return _bRing; }
            set
            {
                SetProperty(ref _bRing, value);
                if (bRing) { bTargetStructure = false; }
            }
        }        

        private int _innerMargin;

        public int InnerMargin
        {
            get { return _innerMargin; }
            set
            {
                SetProperty(ref _innerMargin, value);
                if (SelectedOperation.SelectedOperationEnum == StructureOperationEnum.RING)
                {
                    _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(this);
                }
            }
        }
        private int _outerMargin;

        public int OuterMargin
        {
            get { return _outerMargin; }
            set
            {
                SetProperty(ref _outerMargin, value);
                if (SelectedOperation.SelectedOperationEnum == StructureOperationEnum.RING)
                {
                    _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(this);
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
                if (StructureMargin != 0)
                {
                    _eventAggregator.GetEvent<StructureBuilderChangedEvent>().Publish(this);
                }
            }
        }
        public DelegateCommand IncreaseMarginCommand { get; private set; }
        public DelegateCommand DecreaseMarginCommand { get; private set; }
        public DelegateCommand DeleteStructureOperationCommand { get; private set; }

        public StructureOperationModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            Operations = new ObservableCollection<OperationModel>();
            Structures = new ObservableCollection<StructureModel>();
            IncreaseMarginCommand = new DelegateCommand(OnIncreaseMargin, CanIncreaseMargin);
            DecreaseMarginCommand = new DelegateCommand(OnDecreaseMargin, CanIncreaseMargin);
            DeleteStructureOperationCommand = new DelegateCommand(OnDeleteStructureOperation);
            bTargetStructure = true;
        }

        private void OnDeleteStructureOperation()
        {
            _eventAggregator.GetEvent<DeleteStructureOperationEvent>().Publish(this);
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
            return SelectedStructure != null;
        }
    }
}
