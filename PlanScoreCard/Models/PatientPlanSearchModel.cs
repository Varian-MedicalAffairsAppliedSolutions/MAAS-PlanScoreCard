using PlanScoreCard.Events.HelperWindows;
using PlanScoreCard.Models.ModelHelpers;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Models
{
    public class PatientPlanSearchModel:BindableBase
    {
        public string PatientId { get; set; }
        public string PatientName { get; set; }

        private IEventAggregator _eventAggregator;
        private StructureMatchWarningModel _structureMatchIssue;

        public StructureMatchWarningModel StructureMatchIssue
        {
            get { return _structureMatchIssue; }
            set { SetProperty(ref _structureMatchIssue,value); }
        }
        private bool _bStructureValidationFlag;
        public bool bStructureValidationFlag
        {
            get { return _bStructureValidationFlag; }
            set { SetProperty(ref _bStructureValidationFlag, value); }
        }
        private bool _bStructureValidationWarning;
        public bool bStructureValidationWarning
        {
            get { return _bStructureValidationWarning; }
            set { SetProperty(ref _bStructureValidationWarning, value); }
        }
        public List<PlanModel> Plans { get; set; }
        private PlanModel _selectedPlan;

        public PlanModel SelectedPlan
        {
            get { return _selectedPlan; }
            set 
            { 
                SetProperty(ref _selectedPlan,value); 
            }
        }

        public PatientPlanSearchModel(Patient patient, IEventAggregator eventAggregator)
        {
            Plans = new List<PlanModel>();
            PatientId = patient.Id;
            PatientName = $"{patient.LastName}, {patient.FirstName}";
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<UpdateSelectedPlanValidationEvent>().Subscribe(OnSelectedPlanUpdate);
            _eventAggregator.GetEvent<UpdateSelectedPlanEvent>().Subscribe(OnUpdateSelectedPlan);

            GetPlans(patient);
            //EvaluateStructureMatches();
        }
        //this one is only used to update visuals after plan selections. 
        private void OnUpdateSelectedPlan(StructureModel obj)
        {
            SelectedPlan = null;
            foreach(var plan in Plans)
            {
                if(plan.TemplateStructures.Any(ts=>ts == obj))
                {
                    SelectedPlan = plan;
                    break;
                }
            }
        }

        private void OnSelectedPlanUpdate(PlanModel obj)
        {
            SelectedPlan = obj;
        }

        private void GetPlans(Patient patient)
        {
            foreach(var course in patient.Courses)
            {
                foreach(var plan in course.PlanSetups)
                {
                    var localPlan = new PlanModel(plan, _eventAggregator);
                    Plans.Add(localPlan);
                }
            }
        }
        public void EvaluateStructureMatches(List<StructureModel> scorecardStructures)
        {
            foreach (var plan in Plans)
            {
                plan.EvaluateStructureMatches(scorecardStructures);
            }
            EvaluateFlags();
        }

        public void EvaluateFlags()
        {
            bStructureValidationWarning = false;
            bStructureValidationFlag = false;
            foreach(var plan in Plans)
            {
                plan.EvaluatePlanFlags();
            }
            if (Plans.Any(pl => pl.bStructureValidationFlag))
            {
                bStructureValidationFlag = true;
            }
            else if (Plans.Any(pl => pl.bStructureValidationWarning))
            {
                bStructureValidationWarning = true;
            }
        }
    }
}
