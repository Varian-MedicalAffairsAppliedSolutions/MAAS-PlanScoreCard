using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Models
{
    public class PatientSummaryModel
    {
        public string FirstName { get; set; }
        public string LastName {  get; set; }  
        public string ID {  get; set;}
        public string PatientName {  get; set;}
        public DateTime CreationDate { get; set; }
        public PatientSummaryModel(PatientSummary patientSummary)
        {
            FirstName = patientSummary.FirstName;
            LastName = patientSummary.LastName;
            ID = patientSummary.Id;
            CreationDate = (DateTime)patientSummary.CreationDateTime;
            PatientName = $"{patientSummary.LastName}, {patientSummary.FirstName}";
        }
    }
}
