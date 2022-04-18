using PlanScoreCard.Events;
using PlanScoreCard.Services;
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
            set { SetProperty(ref planId, value); }
        }

        private string courseId;

        public string CourseId
        {
            get { return courseId; }
            set { SetProperty(ref courseId, value); }
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
                //if (bSelected)
                //{
                _eventAggregator.GetEvent<PlanSelectedEvent>().Publish();
                //}
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
                {
                    _eventAggregator.GetEvent<FreePrimarySelectionEvent>().Publish(this);

                    bSelected = bPrimary;
                }
                //don't need to call PlanSelected event because bSelected already calls it.
                //if (bPrimary)
                //{
                //    _eventAggregator.GetEvent<PlanSelectedEvent>().Publish();
                //}
            }
        }
        private bool _bPrimaryEnabled;

        public bool bPrimaryEnabled
        {
            get { return _bPrimaryEnabled; }
            set { SetProperty(ref _bPrimaryEnabled, value); }
        }


        private double? planScore;

        public double? PlanScore
        {
            get { return planScore; }
            set { SetProperty(ref planScore, value); }
        }

        private double maxScore;

        public double MaxScore
        {
            get { return maxScore; }
            set { SetProperty(ref maxScore, value); }
        }
        private double _dosePerFraction;

        public double DosePerFraction
        {
            get { return _dosePerFraction; }
            set { SetProperty(ref _dosePerFraction, value); }
        }
        private int _numberOfFractions;

        public int NumberOfFractions
        {
            get { return _numberOfFractions; }
            set { SetProperty(ref _numberOfFractions, value); }
        }
        public bool bPlanSum;
        public PlanSetup Plan;
        public PlanSum PlanSum;
        private IEventAggregator _eventAggregator;

        public ObservableCollection<StructureModel> Structures { get; set; }
        public PlanModel(PlanningItem plan, IEventAggregator eventAggregator)
        {
            if (plan is PlanSum)
            {
                bPlanSum = true;
                PlanSum = plan as PlanSum;
            }
            //
            //Dose per Fraction is always in Gy
            if (plan is PlanSetup)
            {
                Plan = plan as PlanSetup;
                if ((plan as PlanSetup).TotalDose.Unit == VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.cGy)
                {
                    DosePerFraction = (plan as PlanSetup).DosePerFraction.Dose / 100.0;
                }
                else
                {
                    DosePerFraction = (plan as PlanSetup).DosePerFraction.Dose;
                }
            }
            NumberOfFractions = (plan is PlanSetup) ? (int)(plan as PlanSetup).NumberOfFractions : 0;
            //DoseUnit = (plan is PlanSetup) ? (plan as PlanSetup).TotalDose.UnitAsString : String.Empty;
            _eventAggregator = eventAggregator;
            Structures = new ObservableCollection<StructureModel>();
            GenerateStructures(plan);
            SetParameters();
        }

        private void SetParameters()
        {
            PlanId = bPlanSum ? PlanSum.Id : Plan.Id;
            CourseId = bPlanSum ? PlanSum.PlanSetups.FirstOrDefault().Course.Id : Plan.Course.Id;
            bPrimary = false;
            bSelected = false;
        }

        /// <summary>
        /// Add structures to plan.
        /// </summary>
        private void GenerateStructures(PlanningItem plan)
        {
            foreach (var structure in plan.StructureSet.Structures.Where(x => x.DicomType != "SUPPORT" && x.DicomType != "MARKER"))
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
