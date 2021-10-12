using PlanScoreCard.Events;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Models
{
    public class PlanModel : BindableBase
    {
        public string PlanId { get; set; }
        public string CourseId { get; set; }
        public string DisplayTxt { get; set; }
        private bool _bselected;

        public bool bSelected
        {
            get { return _bselected; }
            set
            {
                SetProperty(ref _bselected, value);
                _eventAggregator.GetEvent<PlanSelectedEvent>().Publish(this);
            }
        }
        private bool _bPrimary;

        public bool bPrimary
        {
            get { return _bPrimary; }
            set
            {
                SetProperty(ref _bPrimary, value);
                _eventAggregator.GetEvent<FreePrimarySelectionEvent>().Publish(bPrimary);
                if (bPrimary)
                {
                    _eventAggregator.GetEvent<PlanSelectedEvent>().Publish(this);
                }
            }
        }
        private bool _bPrimaryEnabled;

        public bool bPrimaryEnabled
        {
            get { return _bPrimaryEnabled; }
            set { SetProperty(ref _bPrimaryEnabled, value); }
        }


        public PlanningItem _plan;
        private IEventAggregator _eventAggregator;

        public ObservableCollection<StructureModel> Structures { get; set; }
        public PlanModel(PlanningItem plan, IEventAggregator eventAggregator)
        {
            _plan = plan;
            _eventAggregator = eventAggregator;
            Structures = new ObservableCollection<StructureModel>();
            GenerateStructures();
        }
        /// <summary>
        /// Add structures to plan.
        /// </summary>
        private void GenerateStructures()
        {
            foreach (var structure in _plan.StructureSet.Structures.Where(x => x.DicomType != "SUPPORT" && x.DicomType != "MARKER"))
            {
                //TODO work on filters for structures
                Structures.Add(new StructureModel
                {
                    StructureId = structure.Id,
                    StructureCode = structure.StructureCodeInfos.FirstOrDefault().Code,
                    StructureComment = structure.Comment
                });
            }
        }
    }
}
