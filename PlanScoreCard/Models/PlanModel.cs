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

        private string planId;

        public string PlanId
        {
            get { return planId; }
            set { SetProperty(ref planId , value); }
        }

        private string courseId;

        public string CourseId
        {
            get { return courseId; }
            set { SetProperty(ref courseId , value); }
        }

        public string DisplayTxt { get; set; }
        
        private bool _bselected;

        public bool bSelected
        {
            get { return _bselected; }
            set
            {
                SetProperty(ref _bselected, value);

                if (!bSelected && bPrimary)
                    bSelected = true;

                _eventAggregator.GetEvent<PlanSelectedEvent>().Publish();
            }
        }
        private bool _bPrimary;

        public bool bPrimary
        {
            get { return _bPrimary; }
            set
            {
                SetProperty(ref _bPrimary, value);

                if (bPrimary)
                    bSelected = bPrimary;

                _eventAggregator.GetEvent<FreePrimarySelectionEvent>().Publish(bPrimary);
                if (bPrimary)
                {
                    _eventAggregator.GetEvent<PlanSelectedEvent>().Publish();
                }
            }
        }
        private bool _bPrimaryEnabled;

        public bool bPrimaryEnabled
        {
            get { return _bPrimaryEnabled; }
            set { SetProperty(ref _bPrimaryEnabled, value); }
        }


        private double planScore;

        public double PlanScore
        {
            get { return planScore; }
            set { SetProperty( ref planScore , value); }
        }

        public PlanSetup Plan;

        private IEventAggregator _eventAggregator;

        public ObservableCollection<StructureModel> Structures { get; set; }
        public PlanModel(PlanningItem plan, IEventAggregator eventAggregator)
        {
            Plan = plan as PlanSetup;
            _eventAggregator = eventAggregator;
            Structures = new ObservableCollection<StructureModel>();
            GenerateStructures();
            SetParameters();
        }

        private void SetParameters()
        {
            PlanId = Plan.Id;
            CourseId = Plan.Course.Id;
            bPrimary = false;
            bSelected = false;
        }

        /// <summary>
        /// Add structures to plan.
        /// </summary>
        private void GenerateStructures()
        {
            foreach (var structure in Plan.StructureSet.Structures.Where(x => x.DicomType != "SUPPORT" && x.DicomType != "MARKER"))
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
