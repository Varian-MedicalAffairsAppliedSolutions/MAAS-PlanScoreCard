using PlanScoreCard.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanScoreCard.Services
{
    public class StructureDictionaryService
    { 

        // Structure Dictionary Collection
        public List<StructureDictionaryModel> StructureDictionary { get; private set; }

        public StructureDictionaryService()
        {

            // Read in the StructureDictionaryPath (Set in the App.config)
            string structureDictionaryPath = ConfigurationManager.AppSettings["StructureDictionaryPath"].ToString();

            StructureDictionary = new List<StructureDictionaryModel>();

            // Populates Structure Dictionary Models
            PopulateStructureModels(structureDictionaryPath);
        }

        // Initialization
        public void PopulateStructureModels(string structureDictionaryPath)
        {

            StreamReader reader = File.OpenText(structureDictionaryPath);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                // If a line does not have a colon, assume that it is an improper entry and move to the next row.
                if (!line.Contains(':'))
                    continue;

                string[] items = line.Split(':');
                string structureID = items[0];   // Here's your integer.
                List<string> synonyms = items[1].Split(',').ToList() ;

                StructureDictionaryModel structureDictionaryModel = new StructureDictionaryModel(structureID,synonyms);
                StructureDictionary.Add(structureDictionaryModel);
            }
            reader.Close();
        }

        // Methods
        public string FindMatch(string potentialSynonm)
        {
            // Returns the first Structure Dictionary that contains a Structure Synonym with the 
            StructureDictionaryModel structureMatch = StructureDictionary.FirstOrDefault(s => s.StructureSynonyms.FirstOrDefault(ss => ss.ToLower() == potentialSynonm.ToLower()) != null);

            if (structureMatch == null)
                return "";

            return structureMatch.StructureID;

        }

        public bool AddSynonym(string structureID, string newSynonym)
        {
            // Checks to see if the synonym already exists in the Dictionary
            StructureDictionaryModel structureMatch = StructureDictionary.FirstOrDefault(s => s.StructureSynonyms.FirstOrDefault(ss => ss.ToLower() == newSynonym.ToLower()) != null);
            if (structureMatch != null)
                return false;

            // Checks to see if the StructureID exists in the Dictionary
            StructureDictionaryModel dictionaryModel = StructureDictionary.FirstOrDefault(s => s.StructureID == structureID);
            if (dictionaryModel == null)
                return false;

            // Adds the new synonym
            dictionaryModel.StructureSynonyms.Add(newSynonym);
            UpdateStructureDictionaryConfig();
            return true;

        }
        
        public bool AddStructure(string structureID)
        {
            // Checks to see if the StructureID exists in the Dictionary
            StructureDictionaryModel dictionaryModel = StructureDictionary.FirstOrDefault(s => s.StructureID == structureID);
            if (dictionaryModel != null)
                return false;

            List<string> synonyms  = new List<string>();
            StructureDictionary.Add(new StructureDictionaryModel(structureID, synonyms));
            UpdateStructureDictionaryConfig();
            return true;
        }

        public bool DeleteStructure(string structureID)
        {
            // Checks to see if the StructureID exists in the Dictionary
            StructureDictionaryModel dictionaryModel = StructureDictionary.FirstOrDefault(s => s.StructureID == structureID);
            if (dictionaryModel == null)
                return false;

            StructureDictionary.Remove(dictionaryModel);
            UpdateStructureDictionaryConfig();
            return true;
        }

        // Update StructureDictionaryConfig

        public void UpdateStructureDictionaryConfig()
        {
            StreamReader reader = File.OpenText(ConfigurationManager.AppSettings["StructureDictionaryPath"].ToString());

            List<string> configFileLines = new List<string>();

            foreach (StructureDictionaryModel structure in StructureDictionary)
            {
                string configLine = "";

                configLine += structure.StructureID + ":";

                foreach (string synonym in structure.StructureSynonyms)
                {
                    configLine += synonym;
                    if (!synonym.Equals(structure.StructureSynonyms.Last()))
                        configLine += ",";
                }
                
                configFileLines.Add(configLine);
            
            }
            string thing = @"C:\Users\jlovis.swim\Documents\GitHub\PlanScoreCard_CORE\PlanScoreCard\" + ConfigurationManager.AppSettings["StructureDictionaryPath"].ToString();
            reader.Close();

            File.WriteAllLines(thing, configFileLines);

        }

        private void MergeDictionary(string dictionaryPath)
        {
            StreamReader reader = File.OpenText(dictionaryPath);
            string line;

            List<StructureDictionaryModel> userDictionary = new List<StructureDictionaryModel>();
            // Populate a Collection from the User-Input Dictionary
            while ((line = reader.ReadLine()) != null)
            {
                // If a line does not have a colon, assume that it is an improper entry and move to the next row.
                if (!line.Contains(':'))
                    continue;

                string[] items = line.Split(':');
                string structureID = items[0];   // Here's your integer.
                List<string> synonyms = items[1].Split(',').ToList();

                StructureDictionaryModel structureDictionaryModel = new StructureDictionaryModel(structureID, synonyms);
                userDictionary.Add(structureDictionaryModel);
            }

            List<StructureDictionaryModel> newDictionary = new List<StructureDictionaryModel>();

            // Compare the two collections and Merge. 
            foreach (StructureDictionaryModel newStructure in userDictionary)
            {
                // Check to see if the StructureDictionary contains this Structure
                StructureDictionaryModel structure = StructureDictionary.FirstOrDefault(d => d.StructureID.ToLower().Equals(newStructure.StructureID.ToLower()));
                
                // If So, Merge Synonyms
                if (structure != null)
                {
                    if (newStructure.StructureSynonyms == null)
                        continue;

                    List<string> newSynonyms = newStructure.StructureSynonyms;
                    foreach (string newSnynonym in newSynonyms)
                    {
                        if (structure.StructureSynonyms == null)
                        {
                            structure.StructureSynonyms.Add(newSnynonym);
                            continue;
                        }

                        if (!structure.StructureSynonyms.Contains(newSnynonym))
                        {
                            structure.StructureSynonyms.Add(newSnynonym);
                            continue;
                        }
                    }

                }
                else                // If Not, Add It
                {
                    StructureDictionary.Add(newStructure);
                }
 

            }
            reader.Close();

            UpdateStructureDictionaryConfig();
        }

        private void OverwriteDictionary(string dictionaryPath)
        {
            StreamReader reader = File.OpenText(dictionaryPath);
            string line;

            List<StructureDictionaryModel> userDictionary = new List<StructureDictionaryModel>();
            // Populate a Collection from the User-Input Dictionary
            while ((line = reader.ReadLine()) != null)
            {
                // If a line does not have a colon, assume that it is an improper entry and move to the next row.
                if (!line.Contains(':'))
                    continue;

                string[] items = line.Split(':');
                string structureID = items[0];   // Here's your integer.
                List<string> synonyms = items[1].Split(',').ToList();

                StructureDictionaryModel structureDictionaryModel = new StructureDictionaryModel(structureID, synonyms);
                userDictionary.Add(structureDictionaryModel);
            }

            reader.Close();

            StructureDictionary = userDictionary;
            UpdateStructureDictionaryConfig();
        }
    }
}
