using PlanScoreCard.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

//[assembly:ESAPIScript(IsWriteable = true)]
namespace PlanScoreCard.Services
{
    public static class StructureGenerationService
    {
        //public static StructureDictionaryService StructureDictionaryService { get; private set; }

        internal static Structure BuildStructureWithESAPI(Application app, string id, string comment, bool bStructureExists, PlanningItem plan)//, StructureDictionaryService structureDictionaryService)
        {
            //StructureDictionaryService = structureDictionaryService;
            try
            {
                Course course = null;
                if (plan is PlanSetup)
                {
                    course = (plan as PlanSetup).Course;
                }
                else if (plan is PlanSum)
                {
                    course = (plan as PlanSum).Course;
                }
                course.Patient.BeginModifications();
                Structure structure = null;
                if (bStructureExists)
                {
                    structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == id);
                }
                else
                {
                    if (plan.StructureSet.CanAddStructure("CONTROL", id))
                    {
                        structure = plan.StructureSet.AddStructure("CONTROL", id);
                    }
                    else
                    {
                        int id_num = 0;
                        string newid = id.Length >= 13 ? $"{id.Substring(0, 12)}{id_num}" : $"{id}{id_num}";
                        if (plan.StructureSet.CanAddStructure("CONTROL", newid))
                        {
                            structure = plan.StructureSet.AddStructure("CONTROL", newid);
                        }
                        while (!plan.StructureSet.CanAddStructure("CONTROL", newid))
                        {
                            id_num++;
                            if (id_num >= 100)
                            {
                                break;
                            }
                            else if (id_num < 10)
                            {
                                newid = id.Length >= 13 ? $"{id.Substring(0, 12)}{id_num}" : $"{id}{id_num}";
                            }
                            else
                            {
                                newid = id.Length >= 13 ? $"{id.Substring(0, 11)}{id_num}" : $"{id}{id_num}";
                            }
                        }
                    }

                }
                if (structure != null)
                {
                    //deconstruct the comment to build the structure.
                    #region OldStructureBuilder
                    //if (comment.StartsWith("("))
                    //{
                    //    //the comment will be in 2 pieces.
                    //    var left_group = comment.Split(')').First().TrimStart('(');
                    //    var right_group = comment.Split('(').Last().TrimEnd(')');
                    //    var operation = comment.Split(')').ElementAt(1).Split(' ').ElementAt(1);
                    //    var segment1 = BuildSegment(plan, left_group);
                    //    if (comment.Split(')').ElementAt(1).Split('(').First().Contains('|'))
                    //    {
                    //        segment1.LargeMargin(Convert.ToInt32(comment.Split(')').ElementAt(1).Split(' ').First().TrimStart('|')));
                    //    }
                    //    var segment2 = BuildSegment(plan, right_group);
                    //    if (comment.Split(')').Last().Contains('|'))
                    //    {
                    //        segment2.LargeMargin(Convert.ToInt32(comment.Split(')').Last().Split(' ').First().TrimStart('|')));
                    //    }
                    //    switch (operation)
                    //    {
                    //        case "AND":
                    //            structure.SegmentVolume = segment1.And(segment2);
                    //            break;
                    //        case "OR":
                    //            structure.SegmentVolume = segment1.Or(segment2);
                    //            break;
                    //        case "SUB":
                    //            structure.SegmentVolume = segment1.Sub(segment2);
                    //            break;
                    //    }
                    //}
                    //else
                    //{
                    //    structure.SegmentVolume = BuildSegment(plan, comment);
                    //}
                    #endregion OldStructureBuilder
                    string SBMessage = string.Empty;
                    var segmentVolume = BuildStructureFromComment(plan, comment, out SBMessage);
                    if (segmentVolume != null)
                    {
                        structure.SegmentVolume = segmentVolume;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Segment could not be generated from comment:\n{comment}\nMessage:{SBMessage}");
                    }
                    if (ConfigurationManager.AppSettings["AddStructures"] == "true")
                    {
                        app.SaveModifications();
                    }
                    return structure;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Structure not generated: {ex}");
                return null;
            }
        }

        internal static SegmentVolume BuildStructureFromComment(PlanningItem plan, string comment, out string output)
        {
            SegmentVolume segment = null;
            //parse this string.
            //List<StructureGrouping> groups = new List<StructureGrouping>();
            //StructureGrouping group = new StructureGrouping(0, 0, String.Empty, 0, String.Empty);
            //groups.Add(group);
            //int comment_index = 0;
            List<StructureGrouping> structureGroups = GetStructureGroupingFromComment(comment);
            int groupCount = 0;
            string groupOperationKeep = String.Empty;
            int groupNumKeep = 0;
            //check if any structure has high resolution segment.
            bool bAnyHighRes = false;
            foreach (var step in structureGroups.OrderByDescending(x => x.groupDepth).ThenBy(x => x.groupNum).SelectMany(x => x.steps))
            {
                var structure = FindStructureByString(plan, step.structureId);
                if (structure == null) { output = $"Could not find structure {step.structureId} from structureset or dictionary"; return null; }
                if (structure.IsHighResolution)
                {
                    bAnyHighRes = true;
                    break;
                }
            }
            List<Structure> structuresToDelete = new List<Structure>();
            foreach (var group in structureGroups.OrderByDescending(x => x.groupDepth).ThenBy(x => x.groupNum))
            {
                //current_depth = group.groupDepth;
                SegmentVolume segmentStep = null;
                string operationKeep = String.Empty;
                foreach (var structureStep in group.steps)
                {
                    if (structureStep == group.steps.First())
                    {
                        var structure = FindStructureByString(plan, structureStep.structureId);
                        if (structure == null) { output = $"Could not find structure {structureStep.structureId} from structureset or dictionary"; return null; }
                        if (bAnyHighRes)
                        {
                            structure = MakeStructureHiRes(plan, structuresToDelete, structure);
                        }
                        segmentStep = !String.IsNullOrEmpty(structureStep.structureMargin?.Trim())
                            ? structure.SegmentVolume.LargeMargin(structureStep.structureMargin)
                            : structure.SegmentVolume;
                    }
                    else
                    {
                        var structure = FindStructureByString(plan, structureStep.structureId);
                        if (structure == null) { output = $"Could not find structure {structureStep.structureId} from structureset or dictionary"; return null; }
                        if (bAnyHighRes)
                        {
                            structure = MakeStructureHiRes(plan, structuresToDelete, structure);
                        }
                        switch (operationKeep)
                        {
                            case "AND":
                                segmentStep = !String.IsNullOrEmpty(structureStep.structureMargin?.Trim()) ?
                                    segmentStep.And(structure.SegmentVolume.LargeMargin(structureStep.structureMargin)) :
                                    segmentStep.And(structure.SegmentVolume);
                                break;
                            case "OR":
                                segmentStep = !String.IsNullOrEmpty(structureStep.structureMargin?.Trim()) ?
                                    segmentStep.Or(structure.SegmentVolume.LargeMargin(structureStep.structureMargin)) :
                                    segmentStep.Or(structure.SegmentVolume);
                                break;
                            case "SUB":
                                segmentStep = !String.IsNullOrEmpty(structureStep.structureMargin?.Trim()) ?
                                   segmentStep.Sub(structure.SegmentVolume.LargeMargin(structureStep.structureMargin)) :
                                   segmentStep.Sub(structure.SegmentVolume);
                                break;
                            default:
                                output = $"Could not determine operation {operationKeep}";
                                return null;
                        }
                    }
                    operationKeep = structureStep.structureOperation;
                }
                if (groupCount == 0)
                {
                    segment = group.groupMargin != 0 ? segmentStep.LargeMargin(group.groupMargin) : segmentStep;
                    groupOperationKeep = group.groupOperation;

                }
                else
                {
                    if (groupNumKeep <= group.groupNum)
                    {
                        switch (groupOperationKeep)
                        {
                            case "AND":
                                segment = group.groupMargin != 0 ?
                                    segment.And(segmentStep.LargeMargin(group.groupMargin)) :
                                    segment.And(segmentStep);
                                break;
                            case "OR":
                                segment = group.groupMargin != 0 ?
                                    segment.Or(segmentStep.LargeMargin(group.groupMargin)) :
                                    segment.Or(segmentStep);
                                break;
                            case "SUB":
                                segment = group.groupMargin != 0 ?
                                    segment.Sub(segmentStep.LargeMargin(group.groupMargin)) :
                                    segment.Sub(segmentStep);
                                break;
                            default:
                                output = $"Could not determine operation {groupOperationKeep}";
                                return null;
                        }
                        groupOperationKeep = group.groupOperation;

                    }
                    else
                    {
                        switch (group.groupOperation)
                        {
                            case "AND":
                                segment = group.groupMargin != 0 ?
                                    segmentStep.LargeMargin(group.groupMargin).And(segment) :
                                    segmentStep.And(segment);
                                break;
                            case "OR":
                                segment = group.groupMargin != 0 ?
                                    segmentStep.LargeMargin(group.groupMargin).Or(segment) :
                                    segmentStep.Or(segment);
                                break;
                            case "SUB":
                                segment = group.groupMargin != 0 ?
                                    segmentStep.LargeMargin(group.groupMargin).Sub(segment) :
                                    segmentStep.Sub(segment);
                                break;
                            default:
                                output = $"Could not determine operation {group.groupOperation}";
                                return null;
                        }
                    }
                }
                groupNumKeep = group.groupNum;
                groupCount++;

            }
            #region groupTests
            //foreach (var s in comment.Split('{'))
            //{
            //    //int depth = 0;
            //    if (String.IsNullOrEmpty(s))
            //    {
            //        depth_keep++;
            //    }
            //    else
            //    {
            //        if (s.Contains("}|"))
            //        {
            //            margin_keep = Convert.ToInt16(s.Split('|').Last().Split(' ').First());
            //        }
            //        groups.Add(new StructureGrouping(num_keep, depth_keep, s.Split('}').First(), margin_keep, operation_keep));
            //        operation_keep = s.Split(' ').ElementAt(s.Split(' ').Count() - 2);
            //        margin_keep = 0;//set margin back to 0. 
            //        depth_keep = depth_keep - s.Count(x => x == '}') - 1;

            //        num_keep++;
            //    }
            //}
            //int depth_keeper = 1;
            //int group_keep = 0;

            //List<SegmentVolume> segments = new List<SegmentVolume>();
            //foreach (var group in groups)
            //{
            //    if (group_keep == 0)
            //    {
            //        //no operation
            //        string structureOperationKeep = String.Empty;
            //        foreach (var s in group.groupComment.Split('<').Skip(1))
            //        {
            //            if (String.IsNullOrEmpty(structureOperationKeep))
            //            {
            //                string id = s.Split('>').First();
            //                Structure structure = FindStructureByString(plan, id);
            //                if (structure == null)
            //                {
            //                    return null;
            //                }
            //                else
            //                {
            //                    segment = structure.SegmentVolume;
            //                    if (s.Contains(">|"))
            //                    {
            //                        segment = segment.Margin(Convert.ToDouble(s.Split('|').Last().Split(' ').First()));
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                string operation = s.Split(' ').ElementAt(1);
            //                string id = s.Split('<').Last();
            //                Structure structure = FindStructureByString(plan, id);
            //                bool hasMargin = false;
            //                double margin = 0;
            //                if (s.Contains(">|"))
            //                {
            //                    hasMargin = Double.TryParse(s.Split('|').Last().Split(' ').First(), out margin);

            //                }
            //                if (structure == null)
            //                {
            //                    return null;
            //                }
            //                else
            //                {
            //                    switch (operation)
            //                    {
            //                        case "AND":
            //                            segment = hasMargin ? segment.And(structure.SegmentVolume.LargeMargin(margin)) : segment.And(structure.SegmentVolume);
            //                            break;
            //                        case "OR":
            //                            segment = hasMargin ? segment.Or(structure.SegmentVolume.LargeMargin(margin)) : segment.Or(structure.SegmentVolume);
            //                            break;
            //                        case "SUB":
            //                            segment = hasMargin ? segment.Sub(structure.SegmentVolume.LargeMargin(margin)) : segment.Sub(structure.SegmentVolume);
            //                            break;
            //                    }
            //                }
            //            }
            //            structureOperationKeep = s.Split(' ').ElementAt(s.Split(' ').Count() - 2);
            //        }
            //        segments.Add(segment);
            //    }
            //    else
            //    {
            //        //operation with existing segment.
            //        if (group.groupDepth > depth_keeper)
            //        {
            //            //must continue building until the next goup. 

            //        }
            //        else
            //        {

            //        }
            //    }
            //    depth_keeper = group.groupDepth;
            //}
            #endregion groupSteps
            //remove hi-res structures generated
            foreach (var deleteStructures in structuresToDelete)
            {
                plan.StructureSet.RemoveStructure(deleteStructures);
            }
            output = "Segment generated";
            return segment;
        }

        internal static List<StructureGrouping> GetStructureGroupingFromComment(string comment)
        {
            int depth_keep = 0;
            int num_keep = 1;
            //int margin_keep = 0;
            //string operation_keep = String.Empty;
            bool structure_starter = false;
            bool checkGroupMargin = false;
            bool checkStructureMargin = false;
            bool outsideStructure = false;
            bool outsideGroup = false;
            bool checkStructureOperation = false;
            bool checkGroupOperation = false;
            //string structure_string = String.Empty;
            int charCount = 0;
            List<StructureGrouping> structureGroups = new List<StructureGrouping>();
            foreach (var s in comment)
            {
                if (s == '{')
                {
                    depth_keep++;
                    outsideGroup = false;
                    if (comment.ElementAt(charCount + 1) != '{')
                    {
                        //new grouping becing created. 
                        structureGroups.Add(new StructureGrouping(num_keep, depth_keep, String.Empty, 0, String.Empty));
                        num_keep++;
                    }
                }
                if (s == '}')
                {
                    outsideStructure = false;
                    if (checkGroupMargin)
                    {
                        checkGroupMargin = false;
                    }
                    if (checkStructureMargin)
                    {
                        checkStructureMargin = false;
                    }
                    depth_keep--;
                    if (comment.Length > charCount + 1 && comment.ElementAt(charCount + 1) != '}')
                    {
                        outsideGroup = true;
                        //outsideStructure = false;
                    }
                }

                if (checkStructureMargin && s == ' ')
                {
                    checkStructureMargin = false;
                }
                if (checkGroupMargin && s == ' ')
                {
                    checkGroupMargin = false;
                }
                if (checkStructureOperation && s == ' ')
                {
                    checkStructureOperation = false;
                    outsideStructure = false;
                }
                if (checkGroupOperation && s == ' ')
                {
                    checkGroupOperation = false;
                    outsideGroup = false;
                }
                if (s == '>')
                {
                    structure_starter = false;
                    outsideStructure = true;
                }
                if (structure_starter)
                {
                    structureGroups.Last().steps.Last().structureId += s;
                }
                if (checkStructureMargin)
                {
                    structureGroups.Last().steps.Last().structureMargin += s.ToString();
                }
                if (checkGroupMargin)
                {
                    structureGroups.Last().groupMargin = Convert.ToInt16(structureGroups.Last().groupMargin.ToString() + s.ToString());
                }
                if (checkStructureOperation)
                {
                    structureGroups.Last().steps.Last().structureOperation += s;
                }
                if (checkGroupOperation)
                {
                    structureGroups.Last().groupOperation += s;
                }
                if (outsideStructure && s == '|')
                {
                    checkStructureMargin = true;
                }
                if (outsideGroup && s == '|')
                {
                    checkGroupMargin = true;
                }
                if (s == '<')
                {
                    structure_starter = true;
                    structureGroups.Last().steps.Add(new StructureStep());
                }

                if (outsideGroup && s == ' ')
                {
                    checkGroupOperation = true;
                }
                if (outsideStructure && s == ' ')
                {
                    checkStructureOperation = true;
                }
                charCount++;
            }

            return structureGroups;
        }
        internal static string StructureModelByString(PlanModel plan, string structureId)
        {
            if (plan.Structures.Any(x => x.StructureId == structureId))
            {
                return plan.Structures.FirstOrDefault(x => x.StructureId == structureId).StructureId;
            }
            StructureDictionaryModel structureDictionary = StructureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.ToLower().Equals(structureId));

            // This means that the template structure Id
            if (structureDictionary != null)
            {
                // Get a collection of all acceptable Structures
                List<string> acceptedStructures = new List<string>();
                acceptedStructures.Add(structureDictionary.StructureID.ToLower());
                if (structureDictionary.StructureSynonyms != null)
                {
                    acceptedStructures.AddRange(structureDictionary.StructureSynonyms.Select(s => s.ToLower()));
                }

                // Gets the Plan Structures
                List<string> planStructrues = plan.Structures.Select(s => s.StructureId.ToLower()).ToList();

                // Finds any matches between the PlanStructures and All Accepted StructIDs
                String structure = String.Empty;
                string matchedStructureID = planStructrues.Intersect(acceptedStructures).FirstOrDefault();
                if (matchedStructureID != null)
                {
                    return plan.Structures.FirstOrDefault(s => s.StructureId.ToLower() == matchedStructureID.ToLower()).StructureId;
                }

            }
            return String.Empty;
        }
        private static Structure MakeStructureHiRes(PlanningItem plan, List<Structure> structuresToDelete, Structure structure)
        {
            bool bHiRes = structure.IsHighResolution;
            string structureId = structure.Id;
            structure = CheckHiResStructre(plan, structureId, out string hiResStructure)
                ? plan.StructureSet.Structures.FirstOrDefault(x => x.Id == hiResStructure)
                : plan.StructureSet.AddStructure("CONTROL", hiResStructure);
            if (bHiRes)
            {
                structure.ConvertToHighResolution();
                structure.SegmentVolume = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == structureId).SegmentVolume;
            }
            else
            {
                structure.SegmentVolume = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == structureId).SegmentVolume;
                structure.ConvertToHighResolution();
            }
            structuresToDelete.Add(structure);
            return structure;
        }

        private static string BuildHiResStructureId(string structureId)
        {
            if (structureId.Length < 12)
            {
                return structureId + "HR";
            }
            else
            {
                return structureId.Substring(0, 11) + "HR";
            }
        }

        private static bool CheckHiResStructre(PlanningItem plan, string structureId, out string hiResStructure)
        {
            if (structureId.Length < 12)
            {
                if (plan.StructureSet.Structures.Any(x => x.Id == structureId + "HR"))
                {
                    hiResStructure = structureId + "HR";
                    return true;
                }
            }
            else
            {
                if (plan.StructureSet.Structures.Any(x => x.Id == structureId.Substring(0, 11) + "HR"))
                {
                    hiResStructure = structureId.Substring(0, 11) + "HR";
                    return true;
                }
            }
            hiResStructure = BuildHiResStructureId(structureId);
            return false;
        }

        private static Structure FindStructureByString(PlanningItem plan, string id)
        {

            if (plan.StructureSet.Structures.Any(x => x.Id == id))
            {
                return plan.StructureSet.Structures.FirstOrDefault(x => x.Id == id);
            }
            StructureDictionaryModel structureDictionary = StructureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.ToLower().Equals(id));

            // This means that the template structure Id
            if (structureDictionary != null)
            {
                // Get a collection of all acceptable Structures
                List<string> acceptedStructures = new List<string>();
                acceptedStructures.Add(structureDictionary.StructureID.ToLower());
                if (structureDictionary.StructureSynonyms != null)
                {
                    acceptedStructures.AddRange(structureDictionary.StructureSynonyms.Select(s => s.ToLower()));
                }

                // Gets the Plan Structures
                List<string> planStructrues = plan.StructureSet.Structures.Select(s => s.Id.ToLower()).ToList();

                // Finds any matches between the PlanStructures and All Accepted StructIDs
                Structure structure = null;
                string matchedStructureID = planStructrues.Intersect(acceptedStructures).FirstOrDefault();
                if (matchedStructureID != null)
                {
                    return plan.StructureSet.Structures.FirstOrDefault(s => s.Id.ToLower() == matchedStructureID.ToLower());
                }

            }
            return null;

        }

        public class StructureGrouping
        {
            public int groupNum { get; set; }
            public int groupDepth { get; set; }
            public string groupComment { get; set; }
            public int groupMargin { get; set; }
            public string groupOperation { get; set; }
            // public SegmentVolume segment { get; set; }
            public List<StructureStep> steps { get; set; }
            public StructureGrouping(int num, int depth, string comment, int margin, string operation)
            {
                steps = new List<StructureStep>();
                groupNum = num;
                groupDepth = depth;
                groupComment = comment;
                groupMargin = margin;
                groupOperation = operation;
            }

        }
        public class StructureStep
        {
            public string structureMargin { get; set; }
            public string structureId { get; set; }
            public string structureOperation { get; set; }

        }

        //private static SegmentVolume BuildSegment(PlanningItem plan, string comment)
        //{
        //    List<int> operation_indexes = new List<int>();
        //    operation_indexes.Add(comment.IndexOf(" RING "));
        //    operation_indexes.Add(comment.IndexOf(" SUB "));
        //    operation_indexes.Add(comment.IndexOf(" OR "));
        //    operation_indexes.Add(comment.IndexOf(" AND "));
        //    if (operation_indexes.Any(x => x != -1))
        //    {
        //        int operation_index = operation_indexes.Where(x => x != -1).Min();
        //        int operation_location = comment.Substring(0, operation_index).Count(x => x == ' ') + 1;
        //        string initial_operation = comment.Split(' ').ElementAt(operation_location);
        //        SegmentVolume return_segment = null;
        //        var base_structure_id = String.Join(" ", comment.Split(' ').Take(operation_location)).Split('|').FirstOrDefault();
        //        var base_margin = comment.Split(' ').FirstOrDefault().Contains('|') ?
        //            Convert.ToInt32(comment.Split(' ').FirstOrDefault().Split('|').Last()) :
        //            0;
        //        //the initial operation will be applied to everything on the right side.

        //        if (!String.IsNullOrEmpty(base_structure_id))
        //        {
        //            var base_structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == base_structure_id);
        //            if (base_structure != null)
        //            {
        //                //loop through and apply changes to structure.
        //                var base_segment = base_structure.SegmentVolume;
        //                int space = 0;
        //                bool bMargin = false;
        //                string structure_operation = "";
        //                SegmentVolume segment = null;

        //                if (initial_operation.Contains("RING"))
        //                {
        //                    var margin_id = comment.Split(' ').ElementAt(operation_location + 1);
        //                    //string innermargin_id = margin_id.Split('*').First();
        //                    double innermargin = Convert.ToDouble(margin_id.Split('*').First());
        //                    double outermargin = Convert.ToDouble(margin_id.Split('*').Last());
        //                    var substruct = plan.StructureSet.AddStructure("CONTROL", $"tempRing999");
        //                    substruct.SegmentVolume = base_segment.LargeMargin(innermargin);
        //                    return_segment = base_segment.LargeMargin(outermargin).Sub(substruct);
        //                    plan.StructureSet.RemoveStructure(substruct);
        //                    return return_segment;
        //                }

        //                else
        //                {
        //                    while (operation_index < comment.Length)
        //                    {
        //                        //try to find the position of the operation within the comment.
        //                        //index if the number of characters while location counts the number of spaces to get to the operation.
        //                        operation_indexes = new List<int>();
        //                        operation_indexes.Add(comment.IndexOf(" RING ", operation_index + 1));
        //                        operation_indexes.Add(comment.IndexOf(" SUB ", operation_index + 1));
        //                        operation_indexes.Add(comment.IndexOf(" AND ", operation_index + 1));
        //                        operation_indexes.Add(comment.IndexOf(" OR ", operation_index + 1));
        //                        var next_operation_index = operation_indexes.All(x => x == -1) ? comment.Length :
        //                            operation_indexes.Where(x => x != -1).Min();
        //                        var next_operation_location = comment.Substring(0, next_operation_index).Count(x => x == ' ') + 1;
        //                        var target_structure_id = next_operation_index == comment.Length ?
        //                            String.Join(" ", comment.Split(' ').Skip(operation_location + 1)) :
        //                            String.Join(" ", comment.Split(' ').Skip(operation_location + 1).Take(next_operation_location - operation_location - 1));
        //                        Structure target_structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == target_structure_id.Split('|').First());
        //                        int margin = 0;
        //                        if (target_structure_id.Contains('|'))
        //                        {
        //                            margin = Convert.ToInt32(target_structure_id.Split('|').Last());
        //                        }
        //                        structure_operation = comment.Split(' ').ElementAt(operation_location);
        //                        if (target_structure != null && !String.IsNullOrEmpty(structure_operation) && segment != null)
        //                        {
        //                            switch (structure_operation)
        //                            {
        //                                case "AND":
        //                                    segment = segment.And(target_structure.SegmentVolume.LargeMargin(margin));
        //                                    break;
        //                                case "OR":
        //                                    segment = segment.Or(target_structure.SegmentVolume.LargeMargin(margin));
        //                                    break;
        //                                case "SUB":
        //                                    segment = segment.Sub(target_structure.SegmentVolume.LargeMargin(margin));
        //                                    break;
        //                            }
        //                        }
        //                        else if (target_structure != null)
        //                        {
        //                            segment = target_structure.SegmentVolume.LargeMargin(margin);
        //                        }

        //                        //string initial_operation = comment.Split(' ').ElementAt(operation_location);
        //                        operation_index = next_operation_index;
        //                        operation_location = next_operation_location;
        //                    }

        //                }
        //                switch (initial_operation)
        //                {
        //                    case "AND":
        //                        return_segment = base_segment.LargeMargin(base_margin).And(segment);
        //                        break;
        //                    case "OR":
        //                        return_segment = base_segment.LargeMargin(base_margin).Or(segment);
        //                        break;
        //                    case "SUB":
        //                        return_segment = base_segment.LargeMargin(base_margin).Sub(segment);
        //                        break;
        //                }

        //            }
        //        }
        //        return return_segment;
        //    }
        //    else
        //    {
        //        if (comment.Contains("|"))
        //        {
        //            return plan.StructureSet.Structures.FirstOrDefault(x => x.Id == comment.Split('|').First()).SegmentVolume.LargeMargin(Convert.ToInt32(comment.Split('|').Last()));
        //        }
        //        else
        //        {
        //            return plan.StructureSet.Structures.FirstOrDefault(x => x.Id == comment).SegmentVolume;
        //        }
        //    }
        //}


    }
    public class StructureMargin
    {
        public int LeftMargin { get; set; }
        public int RightMargin { get; set; }
        public int SupMargin { get; set; }
        public int InfMargin { get; set; }
        public int PostMargin { get; set; }
        public int AntMargin { get; set; }
        public int SymmetricMargin { get; set; }
        public StructureMargin(string marginString)
        {
            if (!String.IsNullOrEmpty(marginString) && marginString.Contains("^"))
            {
                LeftMargin = Convert.ToInt16(marginString.Split('^').First());
                RightMargin = Convert.ToInt16(marginString.Split('^').ElementAt(1));
                SupMargin = Convert.ToInt16(marginString.Split('^').ElementAt(2));
                InfMargin = Convert.ToInt16(marginString.Split('^').ElementAt(3));
                PostMargin = Convert.ToInt16(marginString.Split('^').ElementAt(4));
                AntMargin = Convert.ToInt16(marginString.Split('^').ElementAt(5));
            }
            else
            {
                SymmetricMargin = Convert.ToInt16(marginString);
            }
        }
        public void SubtractMaxMargin()
        {
            LeftMargin = LeftMargin >= 50 ? LeftMargin - 50 : 0;
            RightMargin = RightMargin >= 50 ? RightMargin - 50 : 0;
            AntMargin = AntMargin >= 50 ? AntMargin - 50 : 0;
            SupMargin = SupMargin >= 50 ? SupMargin - 50 : 0;
            InfMargin = InfMargin >= 50 ? InfMargin - 50 : 0;
            PostMargin = PostMargin >= 50 ? PostMargin - 50 : 0;
        }
    }
    static class StructureExtension
    {
        public static SegmentVolume LargeMargin(this SegmentVolume base_segment, string base_margin)
        {
            StructureMargin margin = null;
            if (base_margin.Contains("^"))
            {
                margin = new StructureMargin(base_margin);
            }
            else
            {
                margin = new StructureMargin(null);
                margin.SymmetricMargin = Convert.ToInt16(base_margin.Trim());
            }
            int maxMargin = FindLargestMargin(margin);
            //symmetric margin
            //if (margin.RightMargin == 0)
            //{
                if (margin.SymmetricMargin != 0)
                {
                    if (Math.Abs(maxMargin) < 50)
                    {
                        return base_segment.Margin(margin.LeftMargin);
                    }
                    else
                    {
                        double mmLeft = margin.LeftMargin;
                        SegmentVolume targetLeft = base_segment;
                        while (mmLeft > 50)
                        {
                            mmLeft -= 50;
                            targetLeft = targetLeft.Margin(50);
                        }
                        SegmentVolume result = targetLeft.Margin(mmLeft);
                        return result;
                    }
                }

          
            //}
            if(margin.RightMargin !=0 || margin.LeftMargin !=0 || margin.SupMargin !=0 || margin.InfMargin !=0 || margin.AntMargin !=0 || margin.PostMargin!=0)
            {
                if (maxMargin < 50)
                {
                    return base_segment.AsymmetricMargin(new VMS.TPS.Common.Model.Types.AxisAlignedMargins(VMS.TPS.Common.Model.Types.StructureMarginGeometry.Outer,
                        margin.RightMargin, margin.AntMargin, margin.InfMargin, margin.LeftMargin, margin.PostMargin, margin.SupMargin));
                }
                else
                {
                    SegmentVolume targetLeft = base_segment;
                    int largestMargin = maxMargin;
                    while(largestMargin >= 50)
                    {
                        //largestMargin -= 50;
                        targetLeft = targetLeft.AsymmetricMargin(new VMS.TPS.Common.Model.Types.AxisAlignedMargins(VMS.TPS.Common.Model.Types.StructureMarginGeometry.Outer,
                            margin.RightMargin >= 50 ? 50 : margin.RightMargin,
                            margin.AntMargin >= 50 ? 50 : margin.AntMargin,
                            margin.InfMargin >= 50 ? 50 : margin.InfMargin,
                            margin.LeftMargin >= 50 ? 50 : margin.LeftMargin,
                            margin.PostMargin >= 50 ? 50 : margin.PostMargin,
                            margin.SupMargin >= 50 ? 50 : margin.PostMargin));
                        margin.SubtractMaxMargin();
                        largestMargin = FindLargestMargin(margin);
                    }
                    SegmentVolume result = targetLeft.AsymmetricMargin(new VMS.TPS.Common.Model.Types.AxisAlignedMargins(VMS.TPS.Common.Model.Types.StructureMarginGeometry.Outer,
                        margin.RightMargin,
                        margin.AntMargin,
                        margin.InfMargin,
                        margin.LeftMargin,
                        margin.PostMargin,
                        margin.SupMargin));
                    return result;
                }
            }
            return base_segment;
        }

        private static int FindLargestMargin(StructureMargin margin)
        {
            int maxMargin = Convert.ToInt16(Math.Max(margin.LeftMargin, margin.RightMargin));
            maxMargin = Convert.ToInt16(Math.Max(maxMargin, margin.SupMargin));
            maxMargin = Convert.ToInt16(Math.Max(maxMargin, margin.InfMargin));
            maxMargin = Convert.ToInt16(Math.Max(maxMargin, margin.PostMargin));
            maxMargin = Convert.ToInt16(Math.Max(maxMargin, margin.AntMargin));
            return maxMargin;
        }

        public static SegmentVolume LargeMargin(this SegmentVolume base_segment, double base_margin)
        {
            if (base_margin != 0)
            {
                if (Math.Abs(base_margin) < 50)
                {
                    return base_segment.Margin(base_margin);
                }
                else
                {
                    double mmLeft = base_margin;
                    SegmentVolume targetLeft = base_segment;
                    while (mmLeft > 50)
                    {
                        mmLeft -= 50;
                        targetLeft = targetLeft.Margin(50);
                    }
                    SegmentVolume result = targetLeft.Margin(mmLeft);
                    return result;
                }
            }
            else
            {
                return base_segment;
            }
        }
    }
}

