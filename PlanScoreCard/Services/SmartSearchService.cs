using PlanScoreCard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Services
{
    public static class SmartSearchService
    {
        private const int MaximumResults = 10;
        private readonly IEnumerable<PatientSummaryModel> Patients;

        public static SmartSearchService(IEnumerable<PatientSummaryModel> patientSummaries)
        {
            Patients = patientSummaries;
        }

        public static IEnumerable<PatientSummaryModel> GetMatchingPatients(string search)
        {
            return !string.IsNullOrEmpty(search)
            ? Patients
                .Where(p => IsMatch(p, search))
                .OrderByDescending(p => p.CreationDate)
                .Take(MaximumResults)
            : new PatientSummaryModel[0];
        }

        public IEnumerable<PatientSummaryModel> GetAllPatients()
        {
            return Patients.OrderByDescending(p => p.CreationDate);
        }


        private bool IsMatch(PatientSummaryModel p, string searchText)
        {
            string[] searchTerms = GetSearchTerms(searchText);

            if (searchTerms.Length == 0)         // Nothing typed
            {
                return false;
            }
            else if (searchTerms.Length == 1)    // One word
            {
                return IsMatch(p.ID, searchTerms[0]) ||
                       IsMatch(p.LastName, searchTerms[0]) ||
                       IsMatch(p.FirstName, searchTerms[0]);
            }
            else                                 // Two or more words
            {
                return IsMatchWithLastThenFirstName(p, searchTerms) ||
                       IsMatchWithFirstThenLastName(p, searchTerms);
            }
        }

        private string[] GetSearchTerms(string searchText)
        {
            // Split by whitespace and remove any separators
            return searchText.Split().Select(t => t.Trim(',', ';')).ToArray();
        }

        private bool IsMatch(string actual, string candidate)
        {
            return actual.ToUpper().Contains(candidate.ToUpper());
        }

        private bool IsMatchWithLastThenFirstName(PatientSummaryModel p, string[] searchTerms)
        {
            return IsMatch(p.LastName, searchTerms[0]) &&
                   IsMatch(p.FirstName, searchTerms[1]);
        }

        private bool IsMatchWithFirstThenLastName(PatientSummaryModel p, string[] searchTerms)
        {
            return IsMatch(p.FirstName, searchTerms[0]) &&
                   IsMatch(p.LastName, searchTerms[1]);
        }
    }
}
