using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Models
{
    public class PatientSummaryModel
    {
        public string FirstName { get; set; }
        public string LastName {  get; set; }  
        public string ID {  get; set;}
        public string PatientName {  get; set;}
        public DateTime CreationDate { get; set; }
    }
}
