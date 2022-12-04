using Newtonsoft.Json;
using PlanScoreCard.Events;
using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models.ModelHelpers;
using PlanScoreCard.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

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
        private string _patientId;

        public string PatientId
        {
            get { return _patientId; }
            set { SetProperty(ref _patientId, value); }
        }
        public bool _deselect;
        public string DisplayTxt { get; set; }

        private bool _bselected;

        public bool bSelected
        {
            get { return _bselected; }
            set
            {
                if (!value && !bPrimary && bSelected && !_deselect)
                {
                    bPrimary = true;
                    value = true;
                }
                if (!value && bPrimary && bSelected)
                {
                    bPrimary = false;
                    value = false;
                }
                SetProperty(ref _bselected, value);

                if (!bSelected && bPrimary)
                    bSelected = true;
                //if (bSelected && !bPrimary)
                if (!bPrimary)
                {
                    _eventAggregator.GetEvent<PlanSelectedEvent>().Publish();
                }
                //TODO Implement a way to only hide the score if its deselected, you shouldn't have to score all over again.
                //if(!bSelected && !bPrimary)
                //{
                //    _eventAggregator.GetEvent<RemovePlanFromScoreEvent>().Publish(this);
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


                if (bPrimary && !_deselect)
                {
                    _eventAggregator.GetEvent<FreePrimarySelectionEvent>().Publish(this);
                    if (!bSelected)
                    {
                        bSelected = bPrimary;
                    }
                }
                //don't need to call PlanSelected event because bSelected already calls it.
                //if (bPrizmary)
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
        private DoseValue.DoseUnit _doseUnit;

        public DoseValue.DoseUnit DoseUnit
        {
            get { return _doseUnit; }
            set { _doseUnit = value; }
        }

        private int _numberOfFractions;

        public int NumberOfFractions
        {
            get { return _numberOfFractions; }
            set { SetProperty(ref _numberOfFractions, value); }
        }
        private string _planText;

        public string PlanText
        {
            get { return _planText; }
            set { SetProperty(ref _planText, value); }
        }

        public bool bPlanSum;
        private StructureMatchWarningModel _structureMatchIssue;
        [JsonIgnore]
        public StructureMatchWarningModel StructureMatchIssue
        {
            get { return _structureMatchIssue; }
            set { SetProperty(ref _structureMatchIssue,value); }
        }
        private bool _bStructureValidationFlag;
        [JsonIgnore]
        public bool bStructureValidationFlag
        {
            get { return _bStructureValidationFlag; }
            set { SetProperty(ref _bStructureValidationFlag,value); }
        }
        private bool _bStructureValidationWarning;
        [JsonIgnore]
        public bool bStructureValidationWarning
        {
            get { return _bStructureValidationWarning; }
            set { SetProperty(ref _bStructureValidationWarning,value); }
        }

        //private bool _bPlanScoreValid;

        //public bool bPlanScoreValid
        //{
        //    get { return _bPlanScoreValid; }
        //    set { SetProperty(ref _bPlanScoreValid,value); }
        //}

        //public PlanSetup Plan;
        //public PlanSum PlanSum;
        private IEventAggregator _eventAggregator;

        public ObservableCollection<StructureModel> Structures { get; set; }
        [JsonIgnore]
        public ObservableCollection<StructureModel> TemplateStructures { get; set; }
        private StructureModel _selectedStructureValidation;
        [JsonIgnore]
        public StructureModel SelectedStructureValidation
        {
            get { return _selectedStructureValidation; }
            set { SetProperty(ref _selectedStructureValidation,value); }
        }

        public DelegateCommand DeselectCommand { get; private set; }
        public DelegateCommand ValidatePlanCommand { get; private set; }
        public PlanModel(PlanningItem plan, IEventAggregator eventAggregator)
        {
            TemplateStructures = new ObservableCollection<StructureModel>();
            if (plan is PlanSum)
            {
                bPlanSum = true;
                //PlanSum = plan as PlanSum;
            }
            //
            //Dose per Fraction is always in Gy
            if (plan is PlanSetup)
            {
                //Plan = plan as PlanSetup;
                if ((plan as PlanSetup).TotalDose.Unit == VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.cGy)
                {
                    DosePerFraction = (plan as PlanSetup).DosePerFraction.Dose / 100.0;
                }
                else
                {
                    DosePerFraction = (plan as PlanSetup).DosePerFraction.Dose;
                }
            }
            NumberOfFractions = (plan is PlanSetup) ?
                (plan as PlanSetup)?.NumberOfFractions == null ? 0 : (int)(plan as PlanSetup).NumberOfFractions
                : 0;
            //DoseUnit = (plan is PlanSetup) ? (plan as PlanSetup).TotalDose.UnitAsString : String.Empty;
            _eventAggregator = eventAggregator;
            Structures = new ObservableCollection<StructureModel>();
            DeselectCommand = new DelegateCommand(OnDeselect);
            ValidatePlanCommand = new DelegateCommand(OnValidatePlan);
            GenerateStructures(plan);
            SetParameters(plan);
        }

        private void OnValidatePlan()
        {
            _eventAggregator.GetEvent<UpdateSelectedPlanValidationEvent>().Publish(this);
        }

        private void OnDeselect()
        {
            if (this.bSelected)
            {
                _deselect = true;
                bSelected = false;
                _deselect = false;
            }
        }

        private void SetParameters(PlanningItem plan)
        {
            PlanId = plan.Id;
            CourseId = bPlanSum ? (plan as PlanSum).Course.Id : (plan as PlanSetup).Course.Id;
            PatientId = bPlanSum ? (plan as PlanSum).Course.Patient.Id : (plan as PlanSetup).Course.Patient.Id;
            DoseUnit = bPlanSum ? (plan as PlanSum).PlanSetups.FirstOrDefault().TotalDose.Unit : (plan as PlanSetup).TotalDose.Unit;
            //bPrimary = false;
            //bSelected = false;
            PlanText = $"{CourseId}: {PlanId}";
        }

        /// <summary>
        /// Add structures to plan.
        /// </summary>
        private void GenerateStructures(PlanningItem plan)
        {
            foreach (var structure in plan.StructureSet.Structures.Where(x => x.DicomType != "SUPPORT" && x.DicomType != "MARKER"))
            {
                //TODO work on filters for structures
                Structures.Add(new StructureModel(_eventAggregator)
                {
                    StructureId = structure.Id,
                    StructureCode = structure.StructureCodeInfos.FirstOrDefault().Code,
                    StructureComment = structure.Comment
                });
            }
        }
        public void EvaluateStructureMatches(List<StructureModel> scorecardStructures)
        {
            int tId = 0;
            foreach (var structure in scorecardStructures)
            {
                var localTemplateStructure = new StructureModel(_eventAggregator)
                {
                    StructureId = structure.StructureId,
                    StructureCode = structure.StructureCode,
                    StructureComment = structure.StructureComment,
                    TemplateStructureInt = tId
                };
                tId++;
                localTemplateStructure.EvaluateStructureMatch(Structures.ToList());
                TemplateStructures.Add(localTemplateStructure);
            }
        }

        public void EvaluatePlanFlags()
        {
            bStructureValidationFlag = false;
            bStructureValidationWarning = false;
            if (TemplateStructures.Any(ts => !ts.bValidStructure))
            {
                bStructureValidationFlag = true;
            }
            else if (TemplateStructures.Any(ts => !ts.bDictionaryMatch))
            {
                bStructureValidationWarning = true;
            }
        }
    }
}
