using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using D = VMS.TPS.Common.Model.Types.DoseValuePresentation;
using V = VMS.TPS.Common.Model.Types.VolumePresentation;

namespace DVHViewer2.Models
{
    public class DVHPlotModel : OxyPlot.PlotModel
    {
        private ExternalPlanSetup plan;
        private ScoreCardModel template;
        private IEnumerable<Structure> structures;
        private double rx_dose;
        private StructureDictionaryService _structureDictionaryService;

        public DVHPlotModel(ExternalPlanSetup plan, ScoreCardModel template, IEnumerable<Structure> structures, StructureDictionaryService structureDictionaryService)
        {
            this.plan = plan;
            this.template = template;
            this.structures = structures;
            this.rx_dose = template.NumberOfFractions != 0 && template.DosePerFraction != 0 ?
                template.NumberOfFractions * template.DosePerFraction :
                plan.TotalDose.Unit == VMS.TPS.Common.Model.Types.DoseValue.DoseUnit.Gy ?
                    plan.TotalDose.Dose :
                    plan.TotalDose.Dose / 100.0;
            _structureDictionaryService = structureDictionaryService;
            SetupAxes();
        }

        public ObservableCollection<StructurePlotItem> GetPlotItems(DVHPlotModel plotmodel)
        {
            var retval = new ObservableCollection<StructurePlotItem>();

            foreach (var item in template.ScoreMetrics)
            {

                //please note the DVH view won't build new structures if they don't already exist.
                var structName = item.Structure?.StructureId;
                //these are metrics for getting the struture id
                string matchId = item.PlanModelOverrides.Count() == 0? String.Empty:
                            item.PlanModelOverrides.FirstOrDefault(
                            pmo => pmo.CourseId.Equals((plan as PlanSetup).Course.Id)
                        && pmo.PlanId.Equals(plan.Id)
                        && pmo.PatientId.Equals((plan as PlanSetup).Course.Patient.Id)
                        && pmo.TemplateMetricId.Equals(item.TemplateNumber)
                        && !String.IsNullOrEmpty(pmo.MatchedStructureId))?.MatchedStructureId;
                // Get eclipse structure for volume and color
                var structure = GetStructureFromTemplate(matchId, structName, item.Structure?.TemplateStructureId, item.Structure?.StructureCode, false, String.Empty, plan, false);//structures.Where(x => x.Id.Equals(structName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (structure != null && !structure.IsEmpty)
                {
                    if (retval.Where(x => x.id.Equals(structure.Id)).Count() == 0)
                    {
                        // If structure name not added add it now
                        retval.Add(new StructurePlotItem(structure.Id, this, OxyColor.FromRgb(structure.Color.R, structure.Color.G, structure.Color.B)));
                    } // Else we already have the structure added so add the metric Plot item to it's children

                    var structPlotItem = retval.Where(x => x.id.Equals(structure.Id)).FirstOrDefault();


                    var metricPlotItem = new MetricPlotItem(item, plotmodel, structure, rx_dose, structPlotItem, plan.TotalDose.UnitAsString);

                    structPlotItem.AddMetricPlotItem(metricPlotItem);
                }
            }
            return retval;
        }

        private void SetupAxes()
        {
            // Initialize axes
            this.Axes.Add(new LinearAxis()
            {
                Title = $"Dose [{plan.TotalDose.UnitAsString}]",
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineThickness = 1,
                MinorGridlineColor = OxyColor.FromRgb(15, 15, 15),
                Minimum = -0.001
            });
            //Add Volume Axis
            this.Axes.Add(new LinearAxis()
            {
                Title = "Volume [%]",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                MinorGridlineColor = OxyColor.FromRgb(15, 15, 15),
                Minimum = -0.001
            });
            this.IsLegendVisible = false;
        }

        public LineSeries GetDVHForStruct(Structure str)
        {
            var ser = new LineSeries();
            ser.Title = $"{str.Id} DVH: " + ser.Title;
            ser.Color = OxyColor.FromRgb(str.Color.R, str.Color.G, str.Color.B);
            //ser.Background = OxyColors.Black;
            
            var raw_dvh = plan.GetDVHCumulativeData(str, D.Absolute, V.Relative, 0.1);
            if (raw_dvh == null) { throw new Exception(); }

            foreach (var pt in raw_dvh.CurveData)
            {
                ser.Points.Add(new DataPoint(pt.DoseValue.Dose, pt.Volume));
            }

            return ser;
        }

        public LineSeries GetDVHForId(string id)
        {
            var st = plan.StructureSet.Structures.Where(s => s.Id == id).FirstOrDefault();
            if (st == null) { throw new Exception(); }

            return GetDVHForStruct(st);
        }



        public void PlotScoreBarForMetric(ScoreBarModel sb, bool plot)
        {

            if (sb.Cap1 != null && sb.Cap2 != null)
            {
                if (!plot)
                {
                    if (Annotations.Contains(sb.Cap1))
                    {
                        Annotations.Remove(sb.Cap1);
                        Annotations.Remove(sb.Cap2);
                        Annotations.Remove(sb.ScoreBar);
                    }
                }
                else
                {
                    if (!Annotations.Contains(sb.Cap1))
                    {
                        Annotations.Add(sb.Cap1);
                        Annotations.Add(sb.Cap2);
                        Annotations.Add(sb.ScoreBar);
                    }
                }
            }
            InvalidatePlot(true);
        }
        /// <summary>
        /// Find structure based on templated structure model
        /// </summary>
        /// <param name="id">Structure Id</param>
        /// <param name="code">Structure code</param>
        /// <param name="autoGenerate">Reference to whether the structure is meant to be generated automatically</param>
        /// <param name="comment">Structure Comment that details how to generate the structure.</param>
        /// <param name="plan">Plan whereby to look for the structure.</param>
        /// <returns></returns>
        public Structure GetStructureFromTemplate(string matchedId, string id, string templateId, string code, bool autoGenerate, string comment, PlanningItem plan, bool canBuildStructure)
        {
            // 
            // This method is where we will want to add the logic to the Structure Matching w/ Dictionary
            // - Case Insensitive (Overwrite string.Compare() to automatically do this)
            bool writeable = ConfigurationManager.AppSettings["WriteEnabled"] == "true";
            if (String.IsNullOrEmpty(id) && !String.IsNullOrEmpty(templateId))
            {
                id = templateId;
            }
            // FIRST: Check for an exact Match --> But matchId must not exist. 
            if (id != null && code != null && String.IsNullOrEmpty(matchedId))
            {
                foreach (var s in plan.StructureSet.Structures)
                {
                    //match on code and id, then on code, then on id.
                    if (s.StructureCodeInfos != null && s.StructureCodeInfos.FirstOrDefault().Code == code && s.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                    {
                        return s;
                    }
                }
            }

            //check for structure existence on the manually matched Id.
            if (!String.IsNullOrEmpty(matchedId) && plan.StructureSet.Structures.Any(st => st.Id.Equals(matchedId)))
            {
                //bFromTemplate = true;
                //bFromLocal = true;
                //LocalStructureMatch = matchedId;

                var structure = plan.StructureSet.Structures.FirstOrDefault(st => st.Id.Equals(matchedId));

                return structure;
            }
            // Check for structure existence
            if (plan.StructureSet.Structures.Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {

                var structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                return structure;
            }//If no structure found, try to find structure based on code.
            //next check for structure on templateID.
            if (plan.StructureSet.Structures.Any(x => x.Id.Equals(templateId, StringComparison.OrdinalIgnoreCase)))
            {
                //bFromTemplate = true;
                var structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id.Equals(templateId, StringComparison.OrdinalIgnoreCase));

                return structure;


            }
            // SECOND: If exact match is not there, check to see if it is part of the Structure Dictionary
            //check templateId against structure id. 
            StructureDictionaryModel structureDictionary = _structureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.ToLower().Equals(templateId.ToLower()));

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
                    structure = plan.StructureSet.Structures.FirstOrDefault(s => s.Id.ToLower() == matchedStructureID.ToLower());
                }
                    return structure;
               
            }

            // See if you can find it based on just stucture Code
            if (code != null && code.ToLower() != "control" && code.ToLower() != "ptv" && code.ToLower() != "ctv" && code.ToLower() != "gtv")//do not try to match control structures, they will be mismatched
            {
                if (plan.StructureSet.Structures.Where(x => x.StructureCodeInfos.Any()).Any(y => y.StructureCodeInfos.FirstOrDefault().Code == code) && !autoGenerate)
                {
                    return plan.StructureSet.Structures.Where(x => x.StructureCodeInfos.Any()).FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == code);
                }
            }

            return null;

        }
    }
}

// TODO
// Listview plot
