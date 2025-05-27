using PlanScoreCard.Events;
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
using System.Windows;

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
        public DelegateCommand<Window> GenerateStructureCommand { get; private set; }

        public BuildStructureViewModel(IEventAggregator eventAggregator)
        {
            //_plan = plan;
            _eventAggregator = eventAggregator;
            BuildGroups = new ObservableCollection<BuildStructureGroupModel>();

            //commands
            AddNewGroupingCommand = new DelegateCommand(OnAddNewGrouping, CanAddNewGrouping);
            AddStructureGroupCommand = new DelegateCommand(OnCommitToGrouping);
            GenerateStructureCommand = new DelegateCommand<Window>(OnGenerateStructure);

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
            _eventAggregator.GetEvent<GroupSelectedEvent>().Subscribe(OnNewGroupSelected);
            _eventAggregator.GetEvent<SetStructureBuilderPlanEvent>().Subscribe(SetPlan);
        }

        private bool CanAddNewGrouping()
        {
            //either this should be adding the first group.
            if (BuildGroups.Count == 0)
            {
                return true;
            }
            //or cannot add to group unless all other groups have some filling.
            if (BuildGroups.All(x => !String.IsNullOrEmpty(x.GroupComment)))
            {
                return true;
            }
            return false;
        }

        public void SetPlan(PlanModel plan)
        {
            _plan = plan;
            // GroupSteps.Add(new BuildStructureGroupStepModel(_plan, _eventAggregator, GroupSteps.Count()));
        }
        private void OnGenerateStructure(Window window)
        {
            if (String.IsNullOrWhiteSpace(StructureId))
            {
                MessageBox.Show("No Structure Id put in");
                return;
            }
            if (StructureId.Length > 13)
            {
                MessageBox.Show("Structure Id must be truncated to less than 13 characters");
                return;
            }
            if (BuildGroups.Count() == 0)
            {
                MessageBox.Show("No BuildGroups available");
                return;
            }
            if (_plan.Structures.Any(x => x.StructureId == StructureId))
            {
                MessageBox.Show("Please use a unique structure Id");
                return;
            }
            //verify structure comment.
            var possibleOperations = new List<string> { "AND", "OR", "SUB" };
            //must be an operation between every '>' and '<' (between every structure).
            //must be an operation between ever '}' and '{' (between every group).
            //TODO must find another separator than '>' that is already used for operations between structures.
            foreach (var sOperation in StructureComment.Split('>').Skip(1).Take(StructureComment.Split('>').Count() - 2))
            {
                string subSOperation = sOperation.Split('<').First();//take string from > to the next <
                if (!String.IsNullOrWhiteSpace(subSOperation))
                {
                    if (!possibleOperations.Any(subSOperation.Contains))
                    {
                        MessageBox.Show($"Missing operation between structures at {sOperation}");
                        return;
                    }
                }
            }
            foreach (var gOperation in StructureComment.Split('}').Skip(1).Take(StructureComment.Split('}').Count() - 2))
            {
                if (!String.IsNullOrEmpty(gOperation))
                {
                    string subGOperation = gOperation.Split('{').First();
                    if (!possibleOperations.Any(subGOperation.Contains) && gOperation.Contains('{'))
                    {
                        MessageBox.Show($"Missing operation between groups at {gOperation}");
                        return;
                    }
                }
            }
            var structure = new StructureModel(_eventAggregator)
            {
                StructureId = StructureId,
                TemplateStructureId = StructureId,
                StructureCode = "9999",
                StructureComment = StructureComment,
                AutoGenerated = true,
            };
            //TODO make this a different event because planchanged event is used in other places.
            //_eventAggregator.GetEvent<PlanChangedEvent>().Publish(null);
            //_eventAggregator.GetEvent<AddStructureEvent>().Publish(structure);
            window.Close();
        }

        private void OnNewGroupSelected(BuildStructureGroupModel obj)
        {
            if (SelectedGroup != obj)
            {
                SelectedGroup = obj;
            }
        }

        private void OnMoveGroupRight(BuildStructureGroupModel obj)
        {
            //must have a grouop to the right to replace.
            var group_number = Convert.ToInt16(obj.GroupId.Substring(5));
            if (BuildGroups.Any(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number + 1))
            {
                var moveGroup = BuildGroups.FirstOrDefault(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number + 1);
                moveGroup.GroupId = $"Group{group_number}";
                obj.GroupId = $"Group{group_number + 1}";
                string moveGroupOperation = moveGroup.SelectedOperation;
                moveGroup.SelectedOperation = obj.SelectedOperation;
                obj.SelectedOperation = moveGroupOperation;
                ResetGroups();
            }
        }

        private void OnMoveGroupLeft(BuildStructureGroupModel obj)
        {
            //must have a group to push left.
            var group_number = Convert.ToInt16(obj.GroupId.Substring(5));
            //moving left can do two things, either swap with the number before the current number
            if (BuildGroups.Any(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number - 1))
            {
                var moveGroup = BuildGroups.FirstOrDefault(x => Convert.ToInt16(x.GroupId.Substring(5)) == group_number - 1);
                moveGroup.GroupId = $"Group{group_number}";
                obj.GroupId = $"Group{group_number - 1}";
                string moveGroupOperation = moveGroup.SelectedOperation;
                moveGroup.SelectedOperation = obj.SelectedOperation;
                obj.SelectedOperation = moveGroupOperation;
                ResetGroups();
            }
            else//or moving left can also jump you into another subgroup.
            {
                if (obj.GroupId.Length > 6)//this group is part of a sub-group.
                {
                    int subgroup_number = Convert.ToInt16(obj.GroupId.Substring(5, obj.GroupId.Length - 6));
                    if (obj.GroupId.Last() == '1' && BuildGroups.Where(x => x.GroupId.Length == obj.GroupId.Length).Any(x => Convert.ToInt16(x.GroupId.Substring(5, x.GroupId.Length - 6)) == subgroup_number - 1))
                    {
                        var moveGroup = BuildGroups.OrderBy(x => x.GroupId).Where(x => x.GroupId.Length == obj.GroupId.Length).LastOrDefault(x => Convert.ToInt16(x.GroupId.Substring(5, x.GroupId.Length - 6)) == subgroup_number - 1);
                        obj.GroupId = $"Group{Convert.ToInt16(moveGroup.GroupId.Substring(5)) + 1}";
                        ResetGroups();
                    }
                }
            }

        }

        private void OnDemoteGroup(BuildStructureGroupModel obj)
        {
            //demotion can always add a new number to this.
            int groupDepth = obj.GroupId.Length - 6;
            string currentNum = obj.GroupId.Substring(5);
            string newGroupId = $"Group{currentNum}1";
            obj.GroupId = newGroupId;
            PushGroupsRight(obj, $"{currentNum}1");
            ResetGroups();
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
            int groupNum = 0;
            foreach (var group in groupDetails.OrderBy(x => x.Item1))
            {
                var groupModel = new BuildStructureGroupModel(_plan, _eventAggregator);
                groupModel.GroupId = group.Item1;
                groupModel.SelectedOperation = group.Item3;
                groupModel.GroupMargin = group.Item4;
                groupModel.GroupNumber = groupNum;
                string keepOperation = String.Empty;
                var count = 0;
                if (!String.IsNullOrEmpty(group.Item2))
                {
                    foreach (var step in group.Item2.Split('<').Skip(1))
                    {
                        if (count == 0)
                        {
                            var step1 = groupModel.GroupSteps.First();
                            step1.SelectedStructure = step1.Structures.FirstOrDefault(x => x.StructureId == step.Split('>').FirstOrDefault());
                            if (step.Contains('|'))
                            {
                                step1.StructureMargin = step.Split('|').Last().Split(' ').First();
                            }
                        }
                        else
                        {
                            var currentStep = new BuildStructureGroupStepModel(_plan, _eventAggregator, count);
                            groupModel.GroupSteps.Add(currentStep);

                            currentStep.SelectedOperation = keepOperation;
                            currentStep.SelectedStructure = currentStep.Structures.FirstOrDefault(x => x.StructureId == step.Split('>').FirstOrDefault());
                            if (step.Contains('|'))
                            {
                                currentStep.StructureMargin = step.Split('|').Last().Split(' ').First();
                            }
                        }
                        if (step.Split(' ').Length > 1)
                        {
                            keepOperation = step.Split(' ').ElementAt(step.Split(' ').Length - 2);
                        }
                        count++;
                    }
                }
                BuildGroups.Add(groupModel);
                groupNum++;
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
                for (int i = 0; i < depthdiff; i++)
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
                if (BuildGroups.Count() > groupNum + 1)
                {
                    int nextDepth = depths.ElementAt(groupNum + 1);
                    int depthdiff2 = depth - nextDepth;
                    for (int i = 0; i < depthdiff2; i++)
                    {
                        StructureComment += "}";
                    }
                    if (BuildGroups.ElementAt(groupNum + 1).SelectedOperation != null)
                    {
                        StructureComment += $" {BuildGroups.ElementAt(groupNum + 1).SelectedOperation} ";
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
            AddNewGroupingCommand.RaiseCanExecuteChanged();
        }

        private void OnGroupDelete(BuildStructureGroupModel obj)
        {

            BuildGroups.Remove(obj);
            if (SelectedGroup == obj)
            {
                SelectedGroup = null;
            }
            ResetGroups();
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
            AddNewGroupingCommand.RaiseCanExecuteChanged();
        }
    }
}
