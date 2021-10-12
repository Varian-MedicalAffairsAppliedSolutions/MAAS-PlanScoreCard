using PlanScoreCard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Services
{
    public class PatientSelectService
    {
        private SmartSearchService SmartSearchService;

        public PatientSelectService(SmartSearchService smartSearchService)
        {
            SmartSearchService= smartSearchService; 
        }



        /// <summary>
        /// Get a list of possible patients from a partial string
        /// </summary>
        /// <param name="search">The partial search string</param>
        /// <returns>The list of possible patients</returns>
        public List<PatientSelectModel> GetPatientOptions(string search)
        {
            IEnumerable<PatientSummaryModel> possiblePatients = SmartSearchService.GetMatchingPatients(search);
            return possiblePatients.Select(p => new PatientSelectModel() { ID = p.ID, FirstName = p.FirstName, LastName = p.LastName }).ToList();
        }
    }
}
