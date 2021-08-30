using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;

namespace PlanScoreCard.Services
{
    public static class StructureGenerationService
    {
        internal static Structure BuildStructureWithESAPI(Application app, string id, string comment, bool bStructureExists, PlanningItem plan)
        {
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
                    if (comment.StartsWith("("))
                    {
                        //the comment will be in 2 pieces.
                        var left_group = comment.Split(')').First().TrimStart('(');
                        var right_group = comment.Split('(').Last().TrimEnd(')');
                        var operation = comment.Split(')').ElementAt(1).Split(' ').ElementAt(1);
                        var segment1 = BuildSegment(plan, left_group);
                        if (comment.Split(')').ElementAt(1).Split('(').First().Contains('|'))
                        {
                            segment1.LargeMargin(Convert.ToInt32(comment.Split(')').ElementAt(1).Split(' ').First().TrimStart('|')));
                        }
                        var segment2 = BuildSegment(plan, right_group);
                        if (comment.Split(')').Last().Contains('|'))
                        {
                            segment2.LargeMargin(Convert.ToInt32(comment.Split(')').Last().Split(' ').First().TrimStart('|')));
                        }
                        switch (operation)
                        {
                            case "AND":
                                structure.SegmentVolume = segment1.And(segment2);
                                break;
                            case "OR":
                                structure.SegmentVolume = segment1.Or(segment2);
                                break;
                            case "SUB":
                                structure.SegmentVolume = segment1.Sub(segment2);
                                break;
                        }
                    }
                    else
                    {
                        structure.SegmentVolume = BuildSegment(plan, comment);
                    }
                    if (ConfigurationManager.AppSettings["AddStructures"] == "true")
                    {
                        app.SaveModifications();
                    }
                    return structure;
                }
                return null;
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show($"Structure not generated: {ex}");
                return null;
            }
        }
        private static SegmentVolume BuildSegment(PlanningItem plan, string comment)
        {
            List<int> operation_indexes = new List<int>();
            operation_indexes.Add(comment.IndexOf(" RING "));
            operation_indexes.Add(comment.IndexOf(" SUB "));
            operation_indexes.Add(comment.IndexOf(" OR "));
            operation_indexes.Add(comment.IndexOf(" AND "));
            if (operation_indexes.Any(x => x != -1))
            {
                int operation_index = operation_indexes.Where(x => x != -1).Min();
                int operation_location = comment.Substring(0, operation_index).Count(x => x == ' ') + 1;
                string initial_operation = comment.Split(' ').ElementAt(operation_location);
                SegmentVolume return_segment = null;
                var base_structure_id = String.Join(" ", comment.Split(' ').Take(operation_location)).Split('|').FirstOrDefault();
                var base_margin = comment.Split(' ').FirstOrDefault().Contains('|') ?
                    Convert.ToInt32(comment.Split(' ').FirstOrDefault().Split('|').Last()) :
                    0;
                //the initial operation will be applied to everything on the right side.

                if (!String.IsNullOrEmpty(base_structure_id))
                {
                    var base_structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == base_structure_id);
                    if (base_structure != null)
                    {
                        //loop through and apply changes to structure.
                        var base_segment = base_structure.SegmentVolume;
                        int space = 0;
                        bool bMargin = false;
                        string structure_operation = "";
                        SegmentVolume segment = null;

                        if (initial_operation.Contains("RING"))
                        {
                            var margin_id = comment.Split(' ').ElementAt(operation_location + 1);
                            //string innermargin_id = margin_id.Split('*').First();
                            double innermargin = Convert.ToDouble(margin_id.Split('*').First());
                            double outermargin = Convert.ToDouble(margin_id.Split('*').Last());
                            var substruct = plan.StructureSet.AddStructure("CONTROL", $"tempRing999");
                            substruct.SegmentVolume = base_segment.LargeMargin(innermargin);
                            return_segment = base_segment.LargeMargin(outermargin).Sub(substruct);
                            plan.StructureSet.RemoveStructure(substruct);
                            return return_segment;
                        }

                        else
                        {
                            while (operation_index < comment.Length)
                            {
                                //try to find the position of the operation within the comment.
                                //index if the number of characters while location counts the number of spaces to get to the operation.
                                operation_indexes = new List<int>();
                                operation_indexes.Add(comment.IndexOf(" RING ", operation_index + 1));
                                operation_indexes.Add(comment.IndexOf(" SUB ", operation_index + 1));
                                operation_indexes.Add(comment.IndexOf(" AND ", operation_index + 1));
                                operation_indexes.Add(comment.IndexOf(" OR ", operation_index + 1));
                                var next_operation_index = operation_indexes.All(x => x == -1) ? comment.Length :
                                    operation_indexes.Where(x => x != -1).Min();
                                var next_operation_location = comment.Substring(0, next_operation_index).Count(x => x == ' ') + 1;
                                var target_structure_id = next_operation_index == comment.Length ?
                                    String.Join(" ", comment.Split(' ').Skip(operation_location + 1)) :
                                    String.Join(" ", comment.Split(' ').Skip(operation_location + 1).Take(next_operation_location - operation_location - 1));
                                Structure target_structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id == target_structure_id.Split('|').First());
                                int margin = 0;
                                if (target_structure_id.Contains('|'))
                                {
                                    margin = Convert.ToInt32(target_structure_id.Split('|').Last());
                                }
                                structure_operation = comment.Split(' ').ElementAt(operation_location);
                                if (target_structure != null && !String.IsNullOrEmpty(structure_operation) && segment != null)
                                {
                                    switch (structure_operation)
                                    {
                                        case "AND":
                                            segment = segment.And(target_structure.SegmentVolume.LargeMargin(margin));
                                            break;
                                        case "OR":
                                            segment = segment.Or(target_structure.SegmentVolume.LargeMargin(margin));
                                            break;
                                        case "SUB":
                                            segment = segment.Sub(target_structure.SegmentVolume.LargeMargin(margin));
                                            break;
                                    }
                                }
                                else if (target_structure != null)
                                {
                                    segment = target_structure.SegmentVolume.LargeMargin(margin);
                                }

                                //string initial_operation = comment.Split(' ').ElementAt(operation_location);
                                operation_index = next_operation_index;
                                operation_location = next_operation_location;
                            }

                        }
                        switch (initial_operation)
                        {
                            case "AND":
                                return_segment = base_segment.LargeMargin(base_margin).And(segment);
                                break;
                            case "OR":
                                return_segment = base_segment.LargeMargin(base_margin).Or(segment);
                                break;
                            case "SUB":
                                return_segment = base_segment.LargeMargin(base_margin).Sub(segment);
                                break;
                        }

                    }
                }
                return return_segment;
            }
            else
            {
                if (comment.Contains("|"))
                {
                    return plan.StructureSet.Structures.FirstOrDefault(x => x.Id == comment.Split('|').First()).SegmentVolume.LargeMargin(Convert.ToInt32(comment.Split('|').Last()));
                }
                else
                {
                    return plan.StructureSet.Structures.FirstOrDefault(x => x.Id == comment).SegmentVolume;
                }
            }
        }


    }
    static class StructureExtension
    {
        public static SegmentVolume LargeMargin(this SegmentVolume base_segment, int base_margin)
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

