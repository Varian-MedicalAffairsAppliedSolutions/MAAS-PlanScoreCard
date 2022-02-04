using PlanScoreCard.Events.StructureBuilder;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class BuildStructureGroupModel:BindableBase
    {
        private PlanModel _plan;
        private IEventAggregator _eventAggregator;
        //collections
        public ObservableCollection<BuildStructureGroupStepModel> GroupSteps { get; set; }
        public List<string> Operations { get; set; }
        //properties
        private string _groupId;

        public string GroupId
        {
            get { return _groupId; }
            set { SetProperty(ref _groupId,value); }
        }
        private string _groupComment;

        public string GroupComment
        {
            get { return _groupComment; }
            set { 
                SetProperty(ref _groupComment, value);
                _eventAggregator.GetEvent<UpdateStructureCommentEvent>().Publish();
            }
        }
        private int _groupMargin;


        public int GroupMargin
        {
            get { return _groupMargin; }
            set 
            { 
                SetProperty(ref _groupMargin,value);
                UpdateGroupComment();
                _eventAggregator.GetEvent<UpdateStructureCommentEvent>().Publish();
            }
        }


        private string _selectedOperation;

        public string SelectedOperation
        {
            get { return _selectedOperation; }
            set 
            { 
                SetProperty(ref _selectedOperation,value);
                _eventAggregator.GetEvent<UpdateStructureCommentEvent>().Publish();
            }
        }

        public int GroupNumber { get; set; }
        //commands
        public DelegateCommand DecreaseGroupMarginCommand { get; private set; }
        public DelegateCommand IncreaseGroupMarginCommand { get; private set; }
        public DelegateCommand EditGroupCommand { get; private set; }
        public DelegateCommand MoveLeftCommand { get; private set; }
        public DelegateCommand MoveRightCommand { get; private set; }
        public DelegateCommand DemoteCommand { get; private set; }
        public DelegateCommand PromoteCommand { get; private set; }
        public DelegateCommand DeleteGroupCommand { get; private set; }
        public BuildStructureGroupModel(PlanModel plan, IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            GroupSteps = new ObservableCollection<BuildStructureGroupStepModel>();
            Operations = new List<string> { "AND", "OR", "SUB" };
            //commands
            DecreaseGroupMarginCommand = new DelegateCommand(OnDecreaseGroupMargin);
            IncreaseGroupMarginCommand = new DelegateCommand(OnIncreaseGroupMargin);
            EditGroupCommand = new DelegateCommand(OnEditGroup);
            MoveLeftCommand = new DelegateCommand(OnMoveLeft);
            MoveRightCommand = new DelegateCommand(OnMoveRight);
            DemoteCommand = new DelegateCommand(OnDemote);
            PromoteCommand = new DelegateCommand(OnPromote);
            DeleteGroupCommand = new DelegateCommand(OnDeleteGroup);
            //a groups should start with a single step when its instantiated.
            GroupSteps.Add(new BuildStructureGroupStepModel(_plan, _eventAggregator, GroupSteps.Count()));
            //events
            _eventAggregator.GetEvent<UpdateGroupCommentEvent>().Subscribe(UpdateGroupComment);
            _eventAggregator.GetEvent<AddGroupStepEvent>().Subscribe(OnAddGroupStep);
            _eventAggregator.GetEvent<DeleteGroupStepEvent>().Subscribe(OnDeleteGroupStep);
        }

        private void OnDeleteGroupStep(BuildStructureGroupStepModel obj)
        {
            GroupSteps.Remove(obj);
            UpdateGroupComment();
        }

        private void OnAddGroupStep()
        {
            GroupSteps.Add(new BuildStructureGroupStepModel(_plan, _eventAggregator, GroupSteps.Count()));
        }

        private void OnDeleteGroup()
        {
            _eventAggregator.GetEvent<DeleteGroupEvent>().Publish(this);
        }

        private void OnPromote()
        {
            _eventAggregator.GetEvent<PromoteGroupEvent>().Publish(this);
        }

        private void OnDemote()
        {
            _eventAggregator.GetEvent<DemoteGroupEvent>().Publish(this);
        }

        private void OnMoveRight()
        {
            _eventAggregator.GetEvent<MoveGroupRightEvent>().Publish(this);
        }

        private void OnMoveLeft()
        {
            _eventAggregator.GetEvent<MoveGroupLeftEvent>().Publish(this);
        }

        private void OnEditGroup()
        {
            //send group back to BuildStructureViewModel and reset selectedgroup property.
            _eventAggregator.GetEvent<GroupSelectedEvent>().Publish(this);
        }

        private void OnIncreaseGroupMargin()
        {
            GroupMargin++;
        }

        private void OnDecreaseGroupMargin()
        {
            GroupMargin--;
        }

        private void UpdateGroupComment()
        {
            GroupComment = String.Empty;
            foreach(var step in GroupSteps)
            {
                if(step.SelectedOperation != null)
                {
                    GroupComment += $" {step.SelectedOperation} ";
                }
                if (step.SelectedStructure != null)
                {
                    GroupComment += $"<{step.SelectedStructure.StructureId}>";
                }
                if (step.StructureMargin != 0)
                {
                    GroupComment += $"|{step.StructureMargin}";
                }
            }
        }
    }
}
