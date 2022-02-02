using PlanScoreCard.Events.StructureBuilder;
using PlanScoreCard.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.ViewModels
{
    public class BuildStructureViewModel : BindableBase
    {
        private PlanModel _plan;
        private IEventAggregator _eventAggregator;

        //collections
        public ObservableCollection<BuildStructureGroupModel> BuildGroups { get; set; }
        private BuildStructureGroupModel _selectedGroup;

        public BuildStructureGroupModel SelectedGroup
        {
            get { return _selectedGroup; }
            set { SetProperty(ref _selectedGroup, value); }
        }
        private string _structureComment;

        public string StructureComment
        {
            get { return _structureComment; }
            set { SetProperty(ref _structureComment, value); }
        }
        private string _structureId;

        public string StructureId
        {
            get { return _structureId; }
            set { SetProperty(ref _structureId, value); }
        }


        public string ANDImage { get; set; }
        public string ORImage { get; set; }
        public string SUBImage { get; set; }

        //commands
        public DelegateCommand AddNewGroupingCommand { get; private set; }
        public DelegateCommand AddStructureGroupCommand { get; private set; }

        public BuildStructureViewModel(PlanModel plan, IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            BuildGroups = new ObservableCollection<BuildStructureGroupModel>();

            //commands
            AddNewGroupingCommand = new DelegateCommand(OnAddNewGrouping);
            AddStructureGroupCommand = new DelegateCommand(OnCommitToGrouping);

            //add image files
            string image_file_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources");
            ANDImage = Path.Combine(image_file_path, "AndBoolean.PNG");
            ORImage = Path.Combine(image_file_path, "OrBoolean.PNG");
            SUBImage = Path.Combine(image_file_path, "SubBoolean.PNG");
            //events
            _eventAggregator.GetEvent<DeleteGroupEvent>().Subscribe(OnGroupDelete);
            _eventAggregator.GetEvent<PromoteGroupEvent>().Subscribe(OnPromoteGroup);
            _eventAggregator.GetEvent<UpdateStructureCommentEvent>().Subscribe(UpdateStructureComment);
            _eventAggregator.GetEvent<DemoteGroupEvent>().Subscribe(OnDemoteGroup);
            _eventAggregator.GetEvent<MoveGroupLeftEvent>().Subscribe(OnMoveGroupLeft);
            _eventAggregator.GetEvent<MoveGroupRightEvent>().Subscribe(OnMoveGroupRight);
        }

        private void OnMoveGroupRight(BuildStructureGroupModel obj)
        {
            //must have a grouop to the right to replace.
            var group_number = Convert.ToInt16(obj.GroupId.Substring(5));
            if (BuildGroups.Any(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number+1))
            {
                BuildGroups.FirstOrDefault(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number + 1).GroupId = $"Group{group_number}";
                obj.GroupId = $"Group{group_number + 1}";
                ResetGroups();
            }
        }

        private void OnMoveGroupLeft(BuildStructureGroupModel obj)
        {
            //must have a group to push left.
            var group_number = Convert.ToInt16(obj.GroupId.Substring(5));
            if (BuildGroups.Any(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number - 1))
            {
                BuildGroups.FirstOrDefault(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number - 1).GroupId = $"Group{group_number}";
                obj.GroupId = $"Group{group_number + 1}";
                ResetGroups();
            }
        }

        private void OnDemoteGroup(BuildStructureGroupModel obj)
        {
            //demotion can always add a new number to this.
            int groupDepth = obj.GroupId.Length - 6;
            string currentNum = obj.GroupId.Substring(5);
            string newGroupId = $"Group{currentNum}1";
            PushGroupsRight(obj,$"{currentNum}1");
        }

        private void OnPromoteGroup(BuildStructureGroupModel obj)
        {
            //cannot promote group if the group does not have any demotion. 
            //i.e. Group1 cannot be promoted, but Group 11 can be promoted to group1. 
            //this type of grouping directly impacts the number of braces around it.
            //promotions just chops off the latest number.
            if (obj.GroupId.Length > 6)
            {
                string newGroupNumber = obj.GroupId.Substring(5, obj.GroupId.Length - 6);
                //add 1 to it if number is already taken. 
                if (BuildGroups.Any(x => x.GroupId.Substring(5) == newGroupNumber))
                {
                    newGroupNumber = (Convert.ToInt16(newGroupNumber) + 1).ToString();
                }
                obj.GroupId = $"Group{newGroupNumber}";

                PushGroupsRight(obj, newGroupNumber);
                ResetGroups();
            }
        }

        private void PushGroupsRight(BuildStructureGroupModel obj, string newGroupNumber)
        {
            foreach (var group in BuildGroups.Where(x => x != obj))
            {
                var groupLength = group.GroupId.Length;
                string groupNumber = group.GroupId.Substring(5);
                //check length and check that the numbers before the last match with the 
                if (groupLength == obj.GroupId.Length && group.GroupId.Substring(5, group.GroupId.Length - 6) == obj.GroupId.Substring(5, group.GroupId.Length - 6))
                {
                    if (Convert.ToInt16(groupNumber) >= Convert.ToInt16(newGroupNumber))
                    {
                        group.GroupId = $"Group{Convert.ToInt16(groupNumber) + 1}";
                    }
                }
            }
        }

        private void ResetGroups()
        {
            List<Tuple<string, string, string, int>> groupDetails = new List<Tuple<string, string, string, int>>();
            foreach (var group in BuildGroups)
            {
                groupDetails.Add(new Tuple<string, string, string, int>(group.GroupId, group.GroupComment, group.SelectedOperation, group.GroupMargin));
            }
            BuildGroups.Clear();
            string groupOperation = String.Empty;
            foreach (var group in groupDetails.OrderBy(x => x.Item1))
            {
                var groupModel = new BuildStructureGroupModel(_plan, _eventAggregator);
                groupModel.GroupId = group.Item1;
                groupModel.SelectedOperation = group.Item3;
                groupModel.GroupMargin = group.Item4;
                string keepOperation = String.Empty;
                var count = 0;
                foreach (var step in group.Item2.Split('<').Skip(1))
                {
                    if (count == 0)
                    {
                        var step1 = groupModel.GroupSteps.First();
                        step1.SelectedStructure = step1.Structures.FirstOrDefault(x => x.StructureId == step.Split('>').FirstOrDefault());
                        if (step.Contains('|'))
                        {
                            step1.StructureMargin = Convert.ToInt32(step.Split('|').Last().Split(' ').First());
                        }
                    }
                    else
                    {
                        var currentStep = new BuildStructureGroupStepModel(_plan, _eventAggregator);
                        currentStep.SelectedOperation = keepOperation;
                        currentStep.SelectedStructure = currentStep.Structures.FirstOrDefault(x => x.StructureId == step.Split('>').FirstOrDefault());
                        if (step.Contains('|'))
                        {
                            currentStep.StructureMargin = Convert.ToInt32(step.Split('|').Last().Split(' ').First());
                        }
                    }
                    keepOperation = step.Split(' ').ElementAt(step.Split(' ').Length - 2);
                    count++;
                }
            }
            UpdateStructureComment();
        }

        private void UpdateStructureComment()
        {
            int depthKeep = 0;
            var depths = BuildGroups.Select(x => x.GroupId.Length - 6);
            StructureComment = String.Empty;
            int groupNum = 0;
            foreach (var group in BuildGroups)
            {
                int depth = group.GroupId.Length - 6;

                int depthdiff = depth - depthKeep;
                StructureComment += "{";
                for (int i = 0; i < depth; i++)
                {
                    StructureComment += "{";
                }
                StructureComment += group.GroupComment;
                StructureComment += "}";
                if (group.GroupMargin != 0)
                {
                    StructureComment += $"|{group.GroupMargin}";
                }
                depthKeep = depth;
                if (BuildGroups.Count() > groupNum+1)
                {
                    int nextDepth = depths.ElementAt(groupNum + 1);
                    int depthdiff2 = depth - nextDepth;
                    for (int i = 0; i < depthdiff2; i++)
                    {
                        StructureComment += "}";
                    }
                }
                else
                {
                    for (int i = 0; i < depth; i++)
                    {
                        StructureComment += "}";
                    }
                }
                groupNum++;
            }
        }

        private void OnGroupDelete(BuildStructureGroupModel obj)
        {
            BuildGroups.Remove(obj);
        }

        private void OnCommitToGrouping()
        {
            //update lists for buildgroups.
        }

        private void OnAddNewGrouping()
        {
            var _structureGroup = new BuildStructureGroupModel(_plan, _eventAggregator);
            _structureGroup.GroupId = $"Group{BuildGroups.Count() + 1}";
            _structureGroup.GroupNumber = BuildGroups.Count();
            BuildGroups.Add(_structureGroup);
            SelectedGroup = _structureGroup;
        }
    }
}
