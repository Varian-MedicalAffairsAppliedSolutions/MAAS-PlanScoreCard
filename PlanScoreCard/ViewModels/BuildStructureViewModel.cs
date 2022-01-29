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
    public class BuildStructureViewModel:BindableBase
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
