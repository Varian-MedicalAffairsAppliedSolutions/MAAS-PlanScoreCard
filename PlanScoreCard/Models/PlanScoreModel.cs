using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PlanScoreCard.Models.Internals;
using PlanScoreCard.Services;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace PlanScoreCard.Models
{
    public class PlanScoreModel : BindableBase
    {
        private VMS.TPS.Common.Model.API.Application _app;
        private double _dvhResolution;
        private StructureDictionaryService StructureDictionaryService;


        public Dictionary<string, string> structureMatch;

        public string StructureId { get; set; }
        public string StructureComment { get; set; }
        public string TemplateStructureId { get; set; }
        public string MetricText { get; set; }
        public double ScoreMax { get; set; }
        public string MetricComment { get; set; }
        public string PrintComment { get; set; }
        private bool _bPrintComment;
        private bool _bFromTemplate;

        public bool bFromTemplate
        {
            get { return _bFromTemplate; }
            set { _bFromTemplate = value; }
        }
        private bool _bFromLocal;

        public bool bFromLocal
        {
            get { return _bFromLocal; }
            set { _bFromLocal = value; }
        }

        public bool bPrintComment
        {
            get { return _bPrintComment; }
            set { SetProperty(ref _bPrintComment, value); }
        }
        private bool _bShowPrintComment;

        public bool bShowPrintComment
        {
            get { return _bShowPrintComment; }
            set { SetProperty(ref _bShowPrintComment, value); }
        }
        //private string _localStructureMatch;

        //public string LocalStructureMatch
        //{
        //    get { return _localStructureMatch; }
        //    set { SetProperty(ref _localStructureMatch, value); }
        //}

        public int MetricId { get; set; }
        private double _minXValue;

        public double MinXValue
        {
            get { return _minXValue; }
            set
            {
                _minXValue = value;
                //CheckOutsideBounds();
            }
        }
        private double _maxXValue;

        public double MaxXValue
        {
            get { return _maxXValue; }
            set
            {
                _maxXValue = value;
                //CheckOutsideBounds();
            }
        }

        private string _xAxisLabel;

        public string XAxisLabel
        {
            get { return _xAxisLabel; }
            set
            {
                _xAxisLabel = value;
                //CheckOutsideBounds();
            }
        }
        private bool _bPKColor;

        public bool bPKColor
        {
            get { return _bPKColor; }
            set { _bPKColor = value; }
        }
        public System.Windows.Thickness PKPosition { get; private set; }

        private string _maxScore;

        public string MaxScore
        {
            get { return _maxScore; }
            set { SetProperty(ref _maxScore, value); }
        }
        private string _scoreMean;

        public string ScoreMean
        {
            get { return _scoreMean; }
            set { SetProperty(ref _scoreMean, value); }
        }
        private string _scoreMin;

        public string ScoreMin
        {
            get { return _scoreMin; }
            set { SetProperty(ref _scoreMin, value); }
        }
        private bool _bStatsVis;

        public bool bStatsVis
        {
            get { return _bStatsVis; }
            set { SetProperty(ref _bStatsVis, value); }
        }
        private double _blockWidth;

        public double BlockWidth
        {
            get { return _blockWidth; }
            set { _blockWidth = value; }
        }
        private int fontSize;

        public int FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        private int countBelowMin;

        public int CountBelowMin
        {
            get { return countBelowMin; }
            set { SetProperty(ref countBelowMin, value); }
        }

        private int countAboveMax;

        public int CountAboveMax
        {
            get { return countAboveMax; }
            set { SetProperty(ref countAboveMax, value); }
        }


        private Visibility templateStructureVisibility;

        public Visibility TemplateStructureVisibility
        {
            get { return templateStructureVisibility; }
            set { SetProperty(ref templateStructureVisibility, value); }
        }


        private Visibility outsideMax;

        public Visibility OutsideMax
        {
            get { return outsideMax; }
            set { SetProperty(ref outsideMax, value); }
        }

        private Visibility outsideMin;

        public Visibility OutsideMin
        {
            get { return outsideMin; }
            set { SetProperty(ref outsideMin, value); }
        }

        // These indicate the colours of the out of range indicators.

        private bool isLeftZero;

        public bool IsLeftZero
        {
            get { return isLeftZero; }
            set { SetProperty(ref isLeftZero, value); }
        }

        private bool isRightZero;

        public bool IsRightZero
        {
            get { return isRightZero; }
            set { SetProperty(ref isRightZero, value); }
        }

        private ViewResolvingPlotModel scorePlotModel;

        public ViewResolvingPlotModel ScorePlotModel
        {
            get { return scorePlotModel; }
            set
            {
                SetProperty(ref scorePlotModel, value);
                //CheckOutsideBounds();
            }
        }

        public void CheckOutsideBounds()
        {
            // First get the max and the min of the plot
            if (ScorePlotModel == null)
                return;

            double max = MaxXValue;
            double min = MinXValue;
            //CountBelowMin = 0;
            //CountAboveMax = 0;//reset to 0 so just last time entering this method will count all out of bounds. 
            // This loops through to see the outside Bounds
            foreach (ScoreValueModel scoreValue in ScoreValues)
            {
                // See if the point is above the max
                if (scoreValue.Value > max)
                {
                    CountAboveMax++;
                    OutsideMax = Visibility.Visible;
                }

                // See if the point is below the min
                if (scoreValue.Value < min)
                {
                    CountBelowMin++;
                    OutsideMin = Visibility.Visible;
                }

            }

            if (ScoreValues.Count() == 0)
            {
                return;
            }

            // Set Colours. Access the plot Information. 
            LineSeries series = ScorePlotModel.Series.First() as LineSeries;
            series.Points.OrderBy(o => o.X);
            if (series.Points.Any() && series.Points.Count() > 1)
            {
                DataPoint minValue = series.Points.First();
                DataPoint maxValue = series.Points.Last();

                if (minValue.Y < maxValue.Y)
                {
                    IsLeftZero = true;
                    IsRightZero = false;
                }
                else
                {
                    IsLeftZero = false;
                    IsRightZero = true;
                }
            }

        }

        private ObservableCollection<ScoreValueModel> scoreValues;

        public ObservableCollection<ScoreValueModel> ScoreValues
        {
            get { return scoreValues; }
            set { SetProperty(ref scoreValues, value); }
        }

        public ObservableCollection<PlanScoreColorModel> Colors { get; private set; }
        public ScoreTemplateModel InternalTemplate { get; set; }
        public PlanScoreModel(VMS.TPS.Common.Model.API.Application app, StructureDictionaryService structureDictionaryService)
        {
            _app = app;
            _dvhResolution = Convert.ToDouble(ConfigurationManager.AppSettings["DVHResolution"]);
            ScoreValues = new ObservableCollection<ScoreValueModel>();
            BindingOperations.EnableCollectionSynchronization(ScoreValues, this);
            Colors = new ObservableCollection<PlanScoreColorModel>();
            ScorePlotModel = new ViewResolvingPlotModel();

            StructureDictionaryService = structureDictionaryService;

            OutsideMax = Visibility.Hidden;
            OutsideMin = Visibility.Hidden;
            CountBelowMin = 0;
            CountAboveMax = 0;

            TemplateStructureVisibility = Visibility.Visible;
            bShowPrintComment = true;
        }
        public void InputTemplate(ScoreTemplateModel template)
        {
            InternalTemplate = template;
            MetricText = PlanScoreCalculationServices.GetMetricTextFromTemplate(template);
            SetInitialPlotParameters(template);
        }
        public void BuildPlanScoreFromTemplate(List<PlanningItem> plans, ScoreTemplateModel template, int metricId, string primaryCourseId, string primaryPlanId, bool canBuildStructure, List<ScoreValueModel> localScoreValueCache)
        {
            ScoreMax = template.ScorePoints.Count() == 0 ? -1000 : template.ScorePoints.Max(x => x.Score);
            string id = template.Structure?.StructureId;
            string code = template.Structure?.StructureCode;
            //check manual match ID.
            string matchId = String.Empty;
            if (String.IsNullOrWhiteSpace(template.Structure?.TemplateStructureId))
            {
                TemplateStructureId = id;
            }
            else
            {
                TemplateStructureId = template.Structure.TemplateStructureId;
            }
            if (!String.IsNullOrEmpty(TemplateStructureId))
            {
                template.Structure.TemplateStructureId = TemplateStructureId;
            }
            string templateId = template.Structure?.TemplateStructureId;
            bool auto = template.Structure == null ? false : template.Structure.AutoGenerated;
            string comment = template.Structure?.StructureComment;

            //find out if value is increasing. 
            bool increasing = false;
            increasing = DetermineScorePointDirection(template, increasing);

            //MetricText = PlanScoreCalculationServices.GetMetricTextFromTemplate(template);
            //SetInitialPlotParameters(template);
            MetricId = metricId;
            MetricComment = template.MetricComment;

            foreach (var plan in plans)
            {
                if (template.PlanModelOverrides != null &&
                    template.PlanModelOverrides.Any(pmo => pmo.CourseId.Equals((plan as PlanSetup).Course.Id)
                        && pmo.PlanId.Equals(plan.Id)
                        && pmo.PatientId.Equals((plan as PlanSetup).Course.Patient.Id)
                        && pmo.TemplateMetricId.Equals(template.TemplateNumber)
                        //&& pmo
                        && !String.IsNullOrEmpty(pmo.MatchedStructureId)))
                {
                    matchId = template.PlanModelOverrides.FirstOrDefault(
                        pmo => pmo.CourseId.Equals((plan as PlanSetup).Course.Id)
                    && pmo.PlanId.Equals(plan.Id)
                    && pmo.PatientId.Equals((plan as PlanSetup).Course.Patient.Id)
                    && pmo.TemplateMetricId.Equals(template.TemplateNumber)
                    && !String.IsNullOrEmpty(pmo.MatchedStructureId)).MatchedStructureId;
                }
                // The id and the code are from the template Structure
                Structure structure = String.IsNullOrEmpty(id) && String.IsNullOrEmpty(templateId) ? null : GetStructureFromTemplate(matchId, id, templateId, code, auto, comment, plan, canBuildStructure);
                if (structure != null)
                {
                    template.Structure.StructureId = structure.Id;
                }

                StructureId = structure == null || bFromLocal ? " - " : structure.Id;
                StructureComment = structure == null ? " - " : comment;
                TemplateStructureId = templateId;
                //check to see if scorevalues cache has, then just add to scorevalue for this plan, if not re-calculate it.
                if (localScoreValueCache.Any(sv => sv.PatientId.Equals((plan as PlanSetup).Course.Patient.Id)
                    && sv.CourseId.Equals((plan as PlanSetup).Course.Id)
                    && sv.PlanId.Equals((plan as PlanSetup).Id)
                    && sv.StructureId.Equals(String.IsNullOrEmpty(matchId)? StructureId : matchId)
                    && sv.TemplateNumber == template.TemplateNumber))
                {
                    var scoreValue = localScoreValueCache.First(sv => sv.PatientId.Equals((plan as PlanSetup).Course.Patient.Id)
                    && sv.CourseId.Equals((plan as PlanSetup).Course.Id)
                    && sv.PlanId.Equals((plan as PlanSetup).Id)
                    && sv.StructureId.Equals(String.IsNullOrEmpty(matchId) ? StructureId : matchId)
                    && sv.TemplateNumber == template.TemplateNumber);
                    SetScoreValue(template, primaryCourseId, primaryPlanId, (plan as PlanSetup).Course.Id, plan.Id, scoreValue);
                    ScoreValues.Add(scoreValue);
                }
                else
                {
                    ScoreValueModel scoreValue = new ScoreValueModel();
                    //scoreValue.bVisible = true;
                    scoreValue.OutputUnit = template.OutputUnit;
                    scoreValue.PlanId = plan.Id;

                    if (plan is PlanSetup)
                    {
                        scoreValue.CourseId = (plan as PlanSetup).Course.Id;
                        scoreValue.PatientId = (plan as PlanSetup).Course.Patient.Id;
                    }
                    else if (plan is PlanSum)
                    {
                        scoreValue.CourseId = (plan as PlanSum).Course.Id;
                        scoreValue.PatientId = (plan as PlanSum).Course.Patient.Id;
                    }

                    // Set the Visibility of tyhe TemplateStructureId
                    //moved this outside the foreach loop. 
                    //if (id!=null && id.Equals(TemplateStructureId) && !bFromLocal)
                    //    TemplateStructureVisibility = Visibility.Hidden;
                    if (bFromLocal)
                    {
                        if (!String.IsNullOrEmpty(matchId))
                        {
                            scoreValue.StructureId = matchId;
                        }
                        else
                        {
                            scoreValue.StructureId = StructureId;
                        }
                    }
                    else
                    {
                        scoreValue.StructureId = StructureId;
                    }
                    if (structure != null && plan.Dose != null && !structure.IsEmpty)
                    {
                        if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.DoseAtVolume)
                        {
                            var dvh = plan.GetDVHCumulativeData(structure,
                                template.OutputUnit.Contains("%") ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                                template.InputUnit.Contains("%") ? VolumePresentation.Relative : VolumePresentation.AbsoluteCm3,
                                _dvhResolution);
                            if (dvh != null)
                            {
                                if (Math.Abs(template.InputValue - 100.0) < 0.001) { scoreValue.Value = dvh.MinDose.Dose; }
                                else if (Math.Abs(template.InputValue - 0.0) < 0.001) { scoreValue.Value = dvh.MaxDose.Dose; }
                                else
                                {
                                    scoreValue.Value = dvh.CurveData.FirstOrDefault(x => x.Volume <= template.InputValue + 0.001).DoseValue.Dose;
                                }
                                if (template.OutputUnit != dvh.MaxDose.UnitAsString)
                                {
                                    if (template.OutputUnit == "Gy") { scoreValue.Value = scoreValue.Value / 100.0; }
                                    else { scoreValue.Value = scoreValue.Value * 100.0; }
                                }
                            }
                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.VolumeAtDose)
                        {
                            DVHData dvh = PlanScoreCalculationServices.GetDVHForVolumeType(plan, template, structure, _dvhResolution);
                            if (template.InputUnit != dvh.MaxDose.UnitAsString)
                            {
                                if (template.InputUnit == "Gy")
                                {
                                    scoreValue.Value = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue * 100.0).Volume;
                                }
                                else
                                {
                                    scoreValue.Value = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue / 100.0).Volume;
                                }
                            }
                            else
                            {
                                scoreValue.Value = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue).Volume;
                            }
                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.VolumeOfRegret)
                        {
                            var body = plan.StructureSet.Structures.SingleOrDefault(x => x.DicomType == "EXTERNAL");
                            if (body == null)
                            {
                                System.Windows.MessageBox.Show("No Single Body Structure Found");
                                scoreValue.Value = ScoreMax = scoreValue.Score = -1000;
                                return;
                            }
                            var dvh_body = PlanScoreCalculationServices.GetDVHForVolumeType(plan, template, body, _dvhResolution);
                            var dvh = PlanScoreCalculationServices.GetDVHForVolumeType(plan, template, structure, _dvhResolution);
                            if (template.InputUnit != dvh.MaxDose.UnitAsString)
                            {
                                if (template.InputUnit == "Gy")
                                {
                                    var body_vol = dvh_body.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue * 100).Volume;
                                    var target_vol = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue * 100.0).Volume;
                                    scoreValue.Value = body_vol - target_vol;
                                }
                                else
                                {
                                    var body_vol = dvh_body.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue / 100.0).Volume;
                                    var target_vol = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue / 100.0).Volume;
                                    scoreValue.Value = body_vol - target_vol;
                                }
                            }
                            else
                            {
                                var body_vol = dvh_body.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue).Volume;
                                var target_vol = dvh.CurveData.LastOrDefault(x => x.DoseValue.Dose <= template.InputValue).Volume;
                                scoreValue.Value = body_vol - target_vol;
                            }
                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.ConformationNumber)
                        {
                            //conformation number is (volume at given dose)^2/(total volume @ dose * total target volume)
                            var body = plan.StructureSet.Structures.SingleOrDefault(x => x.DicomType == "EXTERNAL");
                            if (body == null)
                            {
                                System.Windows.MessageBox.Show("No Single Body Structure Found");
                                scoreValue.Value = ScoreMax = scoreValue.Score = -1000; return;
                            }
                            var dvh_body = PlanScoreCalculationServices.GetDVHForVolumeType(plan, template, body, _dvhResolution);
                            var dvh = PlanScoreCalculationServices.GetDVHForVolumeType(plan, template, structure, _dvhResolution);
                            var body_vol = 0.0;
                            var target_vol = 0.0;
                            PlanScoreCalculationServices.GetVolumesFromDVH(template, dvh_body, dvh, out body_vol, out target_vol);
                            if (body_vol == 0 || dvh.CurveData.Max(cd => cd.Volume) == 0 || target_vol == 0)
                            {
                                scoreValue.Value = -1000;
                            }
                            else
                            {
                                scoreValue.Value = Math.Pow(target_vol, 2) / (body_vol * dvh.CurveData.Max(x => x.Volume));
                            }

                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.HomogeneityIndex)
                        {
                            if (plan is PlanSetup)
                            {
                                var dTarget = template.HI_Target;
                                if (template.HI_Target != 0.0 && template.HI_TargetUnit != "%")
                                {
                                    if (template.HI_TargetUnit != (plan as PlanSetup).TotalDose.UnitAsString)
                                    {
                                        if ((plan as PlanSetup).TotalDose.UnitAsString.Contains('c'))
                                        {
                                            //this means templat is in Gy and dose in in cGy
                                            dTarget = template.HI_Target * 100.0;
                                        }
                                        else
                                        {
                                            //plan is in Gy and template is in cgy. 
                                            dTarget = template.HI_Target / 100.0;
                                        }
                                    }
                                    else
                                    {
                                        dTarget = template.HI_Target;
                                    }
                                }
                                var dvh = template.HI_TargetUnit == "%" ?
                                    plan.GetDVHCumulativeData(structure, DoseValuePresentation.Relative, VolumePresentation.Relative, _dvhResolution)
                                    : plan.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute,
                                    VolumePresentation.Relative, _dvhResolution);
                                if (dvh == null) { scoreValue.Value = ScoreMax = scoreValue.Score = -1000; return; }
                                var h_val = template.HI_HiValue;// * dTarget / 100.0;
                                var l_val = template.HI_LowValue;// * dTarget / 100.0;

                                var dHi = dvh.CurveData.FirstOrDefault(x => x.Volume <= h_val).DoseValue.Dose;
                                var dLo = dvh.CurveData.FirstOrDefault(x => x.Volume <= l_val).DoseValue.Dose;
                                //the target dose level has already been converted to the system's dose unit and therefore dHi and dLo do not need to be converted.
                                //if (template.HI_TargetUnit != (plan as PlanSetup).TotalDose.UnitAsString)
                                //{
                                //    if ((plan as PlanSetup).TotalDose.UnitAsString.Contains('c'))
                                //    {
                                //        dHi = dHi* 100.0;
                                //        dLo = dLo* 100.0;
                                //    }
                                //    else
                                //    {
                                //        dHi= dHi / 100.0;
                                //        dLo = dLo / 100.0;
                                //    }
                                //}
                                scoreValue.Value = template.OutputUnit == "%" ? (dHi - dLo) / (dTarget - (plan as PlanSetup).TotalDose.Dose) : (dHi - dLo) / dTarget;
                            }
                            else
                            {
                                //HI not yet supported for plansums.
                                scoreValue.Value = -1000;
                            }
                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.ConformityIndex)
                        {
                            var body = plan.StructureSet.Structures.SingleOrDefault(x => x.DicomType == "EXTERNAL");
                            if (body == null)
                            {
                                System.Windows.MessageBox.Show("No single body structure found.");
                                scoreValue.Value = ScoreMax = scoreValue.Score = -1000; return;
                            }
                            //goahead and make the DVH absolute volume for conformity index (not saved in template). 
                            template.OutputUnit = "cc";
                            var dvh_body = PlanScoreCalculationServices.GetDVHForVolumeType(plan, template, body, _dvhResolution);
                            var dvh = PlanScoreCalculationServices.GetDVHForVolumeType(plan, template, structure, _dvhResolution);
                            var body_vol = 0.0;
                            var target_vol = 0.0;
                            PlanScoreCalculationServices.GetVolumesFromDVH(template, dvh_body, dvh, out body_vol, out target_vol);
                            scoreValue.Value = body_vol / structure.Volume;
                            template.OutputUnit = String.Empty;

                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.InhomogeneityIndex)
                        {
                            var dvh = plan.GetDVHCumulativeData(structure, DoseValuePresentation.Absolute,
                                VolumePresentation.Relative, _dvhResolution);
                            if (dvh == null) { scoreValue.Value = ScoreMax = scoreValue.Score = -1000; return; }
                            scoreValue.Value = (dvh.MaxDose.Dose - dvh.MinDose.Dose) / dvh.MeanDose.Dose;
                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.ModifiedGradientIndex)
                        {
                            //plan.DoseValuePresentation = DoseValuePresentation.Absolute;
                            //var doseUnit = plan.Dose.DoseMax3D.UnitAsString;
                            var dhi = template.HI_HiValue;
                            var dlo = template.HI_LowValue;
                            var unit = template.InputUnit;
                            var dvh = plan.GetDVHCumulativeData(structure,
                                unit.Contains("%") ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                                VolumePresentation.AbsoluteCm3,
                                _dvhResolution);
                            if (dvh == null) { scoreValue.Value = ScoreMax = scoreValue.Score = -1000; return; }
                            var doseUnit = dvh.MaxDose.UnitAsString;
                            if (unit != doseUnit)
                            {
                                if (unit.StartsWith("c"))
                                {
                                    //unit is cGy and system unit is in Gy.
                                    dhi = dhi / 100.0;
                                    dlo = dlo / 100.0;
                                }
                                else
                                {
                                    //unit is in Gy and system unit is in cGy
                                    dhi = dhi * 100.0;
                                    dlo = dlo * 100.0;
                                }
                            }
                            var vDLo = dvh.CurveData.FirstOrDefault(x => x.DoseValue.Dose >= dlo).Volume;
                            var vDHi = dvh.CurveData.FirstOrDefault(x => x.DoseValue.Dose >= dhi).Volume;
                            scoreValue.Value = vDLo / vDHi;
                        }
                        else if ((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.DoseAtSubVolume)
                        {
                            var specVolume = template.InputValue;
                            var structureVolume = structure.Volume;
                            var unit = template.OutputUnit;
                            var dvh = plan.GetDVHCumulativeData(structure,
                                unit.Contains("%") ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                                VolumePresentation.AbsoluteCm3, _dvhResolution);
                            if (dvh == null) { scoreValue.Value = ScoreMax = scoreValue.Score = -1000; return; }
                            var doseValue = dvh.CurveData.FirstOrDefault(x => x.Volume <= structureVolume - specVolume).DoseValue;
                            var doseUnit = doseValue.UnitAsString;
                            scoreValue.Value = doseValue.Dose;
                            if (unit != doseUnit)
                            {
                                if (unit.StartsWith("c"))
                                {
                                    //unit is in cGy and dvh is in Gy
                                    scoreValue.Value = doseValue.Dose * 100.0;
                                }
                                else
                                {
                                    scoreValue.Value = doseValue.Dose / 100.0;
                                }
                            }

                        }
                        else
                        {
                            if (String.IsNullOrEmpty(template.OutputUnit))
                            {
                                MessageBox.Show($"No output unit for metric {template.MetricType} on {template.Structure.StructureId}");
                                scoreValue.Value = -1000;
                            }
                            else
                            {
                                var dvh = plan.GetDVHCumulativeData(structure,
                                    template.OutputUnit.Contains("%") ? DoseValuePresentation.Relative : DoseValuePresentation.Absolute,
                                    VolumePresentation.Relative,
                                    _dvhResolution);
                                if (template.MetricType.Contains("Min"))
                                {
                                    scoreValue.Value = dvh.MinDose.Dose;
                                }
                                else if (template.MetricType.Contains("Max"))
                                {
                                    scoreValue.Value = dvh.MaxDose.Dose;
                                }
                                else if (template.MetricType.Contains("Mean"))
                                {
                                    scoreValue.Value = dvh.MeanDose.Dose;
                                }
                                if (template.OutputUnit != dvh.MaxDose.UnitAsString)
                                {
                                    if (template.OutputUnit == "Gy") { scoreValue.Value = scoreValue.Value / 100.0; }
                                    else { scoreValue.Value = scoreValue.Value * 100.0; }
                                }
                            }
                        }
                        if (template.ScorePoints.Any())
                        {
                            scoreValue.Score = PlanScoreCalculationServices.GetScore(template.ScorePoints, increasing, scoreValue.Value);
                        }
                        else { scoreValue.Score = -1000; }
                    }
                    else
                    {
                        scoreValue.Score = 0.0;
                        scoreValue.Value = -1000;
                    }

                    SetScorePlotModel(template, primaryCourseId, primaryPlanId, increasing, (plan as PlanSetup).Course.Id, plan.Id, scoreValue);
                    scoreValue.TemplateNumber = template.TemplateNumber;
                    ScoreValues.Add(scoreValue);
                    localScoreValueCache.Add(scoreValue);
                }
            }

            GetTemplateStructureVisibility();
            //CheckOutsideBounds();

            if (ScoreValues.Count() > 1)
            {
                GetScoreValueStats();
            }
            UpdateScorePlotModel();
        }
        internal void SetScoreValue(ScoreTemplateModel template, string primaryCourseId, string primaryPlanId, string currentCourseId, string currentPlanId, ScoreValueModel scoreValue)
        {
            bool increasing = false;
            increasing = DetermineScorePointDirection(template, increasing);
            SetScorePlotModel(template, primaryCourseId, primaryPlanId, increasing, currentCourseId, currentPlanId, scoreValue);
            //the following are commented out as they're performed after the structure has been matched.
            //GetTemplateStructureVisibility();
            //if (ScoreValues.Count() > 1)
            //{
            //    GetScoreValueStats();
            //}
            //UpdateScorePlotModel();
        }
        private static bool DetermineScorePointDirection(ScoreTemplateModel template, bool increasing)
        {
            if (template.ScorePoints.Count() > 0)
            {
                increasing = template.ScorePoints.ElementAt(
                  Array.IndexOf(template.ScorePoints.Select(x => x.PointX).ToArray(),
                  template.ScorePoints.Min(x => x.PointX))).Score <
                  template.ScorePoints.ElementAt(
                      Array.IndexOf(template.ScorePoints.Select(x => x.PointX).ToArray(),
                      template.ScorePoints.Max(x => x.PointX))).Score;
            }

            return increasing;
        }

        private void SetScorePlotModel(ScoreTemplateModel template, string primaryCourseId, string primaryPlanId, bool increasing, string currentCourseId, string currentPlanId, ScoreValueModel scoreValue)
        {
            if (scoreValue.Value != -1000 && template.ScorePoints.Count() > 0)
            {
                //break scorepoints into 2 groups, before and after the variation.
                //this one sets marker color.
                //this method is changed to only show the marker.
                bool checkCourse = false;
                if (!String.IsNullOrEmpty(primaryPlanId) && !String.IsNullOrEmpty(primaryCourseId))
                {
                    checkCourse = primaryCourseId == currentCourseId;
                    //if (plan is PlanSum)
                    //{
                    //    checkCourse = (plan as PlanSum).PlanSetups.Any(x => x.Course.Id == primaryCourseId);
                    //}
                    //else
                    //{
                    //    checkCourse = (plan as PlanSetup).Course.Id == primaryCourseId;
                    //}
                }
                var ScorePointSeries = new LineSeries
                {
                    //Color = ScorePlotModel.Series.Any(x => !String.IsNullOrWhiteSpace(x.Title) && x.Title.Contains("Marker")) ? OxyColors.Black : PlanScorePlottingServices.GetColorFromMetric(scoreValue.Score, template),
                    //Color = plan.Id == primaryPlanId && checkCourse ? PlanScorePlottingServices.GetColorFromMetric(scoreValue.Score, template) : OxyColors.Black,
                    Color = currentPlanId == primaryPlanId && checkCourse ? PlanScorePlottingServices.GetColorFromMetric(scoreValue.Score, template) : OxyColors.Black,
                    MarkerType = MarkerType.Plus,
                    MarkerStroke = currentPlanId == primaryPlanId && checkCourse ? PlanScorePlottingServices.GetColorFromMetric(scoreValue.Score, template) : OxyColors.Black,
                    //MarkerStroke = ScorePlotModel.Series.Any(x => !String.IsNullOrWhiteSpace(x.Title) && x.Title.Contains("Marker")) ? OxyColors.Black : PlanScorePlottingServices.GetColorFromMetric(scoreValue.Score, template),
                    //MarkerSize = ScorePlotModel.Series.Any(x => !String.IsNullOrWhiteSpace(x.Title) && x.Title.Contains("Marker")) ? 6 : 12,
                    MarkerSize = currentPlanId == primaryPlanId && checkCourse ? 12 : 6,
                    Title = "Marker"
                };
                //add to the plot
                ScorePointSeries.Points.Add(new DataPoint(scoreValue.Value, scoreValue.Score));
                ScorePlotModel.Series.Add(ScorePointSeries);
                //get colors frm pk template if it exists.
                switch (template.ScorePoints.Where(x => x.Colors.Count() > 2).Count())
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        BlockWidth = 100.0;
                        FontSize = 10;
                        break;
                    case 5:
                        BlockWidth = 80.0;
                        FontSize = 9;
                        break;
                    case 6:
                    case 7:
                        BlockWidth = 60.0;
                        FontSize = 7;
                        break;
                    case 8:
                        BlockWidth = 40.0;
                        FontSize = 5;
                        break;
                    default:
                        BlockWidth = 20.0;
                        FontSize = 4;
                        break;
                }
                if (template.ScorePoints.Count() > 0 && Colors.Count() == 0 && currentPlanId == primaryPlanId)
                {
                    foreach (var score in template.ScorePoints)
                    {
                        if (score.Colors.Count > 0 && !score.Colors.All(x => x == 0))
                        {
                            bPKColor = true;
                            var pkColor = new PlanScoreColorModel(score.Colors, score.Label);
                            Colors.Add(pkColor);
                        }
                    }
                    foreach (var color in Colors)
                    {
                        if ((increasing && color != Colors.LastOrDefault(x => scoreValue.Score >= x.ColorValue)) ||
                            (!increasing && color != Colors.FirstOrDefault(x => x.ColorValue <= scoreValue.Score)))
                        {
                            //    if (color != PKColors.LastOrDefault(x => spoint_value.Score >= x.PKColorValue))
                            //    {
                            if (color.ColorValue > scoreValue.Score)
                            {
                                color.PlanScoreBackgroundColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(
                                    Convert.ToByte(211),
                                    Convert.ToByte(211),
                                    Convert.ToByte(211)));
                            }
                            else
                            {
                                color.PlanScoreBackgroundColor = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(
                                   Convert.ToByte(105),
                                   Convert.ToByte(105),
                                   Convert.ToByte(105)));
                            }
                        }

                    }
                    if (template.ScorePoints.Any(x => x.Colors.Count() > 0))
                    {
                        PKPosition = new System.Windows.Thickness(PlanScoreCalculationServices.CalculatePKPosition(Colors.Where(x => x.Colors.Count() > 0).ToList(), increasing, scoreValue.Score, BlockWidth), 0, 0, 0);
                    }
                }

            }
        }
        internal void AddPointToPlotModel(int metricId, ScoreValueModel scoreValue)
        {
            if (!String.IsNullOrEmpty(scoreValue.StructureId))
            {
                if(ScorePlotModel.Series.Any(s=>s.Title == "TempMarker"))
                {
                    ScorePlotModel.Series.Remove(ScorePlotModel.Series.FirstOrDefault(s => s.Title == "TempMarker"));
                }
                var scorePointSeries = new LineSeries
                {
                    Color = OxyColors.Black,
                    MarkerType = MarkerType.Plus,
                    MarkerStroke = OxyColors.Black,
                    MarkerSize = 6,
                    Title = "TempMarker"
                };
                scorePointSeries.Points.Add(new DataPoint(scoreValue.Value, scoreValue.Score));
                ScorePlotModel.Series.Add(scorePointSeries);
                UpdateScorePlotModel();
            }
        }

        internal void UpdateScorePlotModel()
        {
            ScorePlotModel.InvalidatePlot(true);
        }

        internal void GetTemplateStructureVisibility()
        {
            if (!StructureId.Equals(TemplateStructureId, StringComparison.OrdinalIgnoreCase))//in the event that the above is unecessary
            {
                TemplateStructureVisibility = Visibility.Visible;//still show template structure Id if structure Id doesn't match.
            }
            else
            {
                TemplateStructureVisibility = Visibility.Hidden;
            }
        }

        public void GetScoreValueStats()
        {
            bStatsVis = true;
            MaxScore = $"Max={ScoreValues.Max(x => x.Score):F2}";
            ScoreMin = $"Min={ScoreValues.Min(x => x.Score):F2}";
            ScoreMean = $"Mean={ScoreValues.Average(x => x.Score):F2}";
        }

        /// <summary>
        /// Set up plot from template
        /// </summary>
        /// <param name="template">plot from template</param>
        private void SetInitialPlotParameters(ScoreTemplateModel template)
        {
            //plotting 
            if (template.ScorePoints.Count() > 1)
            {
                MinXValue = template.ScorePoints.Min(x => x.PointX);
                MaxXValue = template.ScorePoints.Max(x => x.PointX);
                //CheckOutsideBounds();
                XAxisLabel = template.ScorePoints.Any(x => x.Variation) ?
                    $"Variation @ {template.ScorePoints.FirstOrDefault(x => x.Variation).PointX}{template.OutputUnit}"
                    : $"{((MetricTypeEnum)Enum.Parse(typeof(MetricTypeEnum), template.MetricType) == MetricTypeEnum.HomogeneityIndex ? MetricText : $"{MetricText.Split(' ').FirstOrDefault()} [{template.OutputUnit}]")}";
            }
            ScorePlotModel.Series.Clear();
            ScorePlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                IsAxisVisible = false,
                //TicklineColor = OxyColors.Transparent,
                //MajorTickSize = 0,
                //MinorTickSize = 0,
                //FontSize = 8,
                //AxisTitleDistance = 0,
                ////Minimum = template.ScorePoints.Min(x=>x.PointX)-0.1,
                ////Maximum = template.ScorePoints.Max(x=>x.PointX)+0.1,
                AbsoluteMinimum = template.ScorePoints.Count() > 1 ? template.ScorePoints.Min(x => x.PointX) : 0,
                AbsoluteMaximum = template.ScorePoints.Count() > 1 ? template.ScorePoints.Max(x => x.PointX) : 1,
                ////DataMinimum = template.ScorePoints.Min(x=>x.PointX),
                ////DataMaximum = template.ScorePoints.Max(x=>x.PointX),
                //MajorStep = template.ScorePoints.Max(x => x.PointX) - template.ScorePoints.Min(x => x.PointX),
                //MinorStep = template.ScorePoints.Max(x => x.PointX) - template.ScorePoints.Min(x => x.PointX)
            });
            ScorePlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                IsAxisVisible = false
            });
            ScorePlotModel.IsLegendVisible = false;
            if (template.ScorePoints.Any(x => x.Variation))
            {
                var PointSeriesAbove = new LineSeries
                {
                    Color = OxyColors.Green
                };
                foreach (var spoint in template.ScorePoints.Where(x => x.Score >= template.ScorePoints.SingleOrDefault(y => y.Variation).Score).OrderBy(x => x.PointX))
                {

                    //add to the plot
                    PointSeriesAbove.Points.Add(new DataPoint(spoint.PointX, spoint.Score));
                }
                ScorePlotModel.Series.Add(PointSeriesAbove);

                var PointSeriesBelow = new LineSeries
                {
                    Color = OxyColors.Yellow
                };
                foreach (var spoint in template.ScorePoints.Where(x => x.Score <= template.ScorePoints.SingleOrDefault(y => y.Variation).Score).OrderBy(x => x.PointX))
                {
                    //add to the plot
                    PointSeriesBelow.Points.Add(new DataPoint(spoint.PointX, spoint.Score));
                }
                ScorePlotModel.Series.Add(PointSeriesBelow);
            }
            else
            {
                //make it all green if there is no variation.
                var PointSeriesAllGreen = new LineSeries
                {
                    Color = OxyColors.Green
                };
                foreach (var spoint in template.ScorePoints.OrderBy(x => x.PointX))
                {
                    //add to the plot
                    PointSeriesAllGreen.Points.Add(new DataPoint(spoint.PointX, spoint.Score));
                }
                ScorePlotModel.Series.Add(PointSeriesAllGreen);
            }
            // ScorePlotModel.InvalidatePlot(true);
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
                bFromLocal = true;
                //LocalStructureMatch = matchedId;

                var structure = plan.StructureSet.Structures.FirstOrDefault(st => st.Id.Equals(matchedId));
                if (structure != null && !structure.IsEmpty)
                {
                    return structure;
                }
                else if (structure.IsEmpty && autoGenerate && writeable && canBuildStructure)
                {
                    var new_structure = StructureGenerationService.BuildStructureWithESAPI(_app, structure.Id, comment, true, plan, StructureDictionaryService);
                    return new_structure;
                }
                else
                {
                    return structure;
                }
            }
            // Check for structure existence
            if (plan.StructureSet.Structures.Any(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {

                var structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

                if (structure != null && !structure.IsEmpty)
                {
                    return structure;
                }
                else if (structure.IsEmpty && autoGenerate && writeable && canBuildStructure)//generate structure if empty.
                {
                    var new_structure = StructureGenerationService.BuildStructureWithESAPI(_app, structure.Id, comment, true, plan, StructureDictionaryService);
                    return new_structure;
                }
                else//return empty structure. 
                {
                    //what do do with an empty structure.
                    return structure;
                }

            }//If no structure found, try to find structure based on code.
            //next check for structure on templateID.
            if (plan.StructureSet.Structures.Any(x => x.Id.Equals(templateId, StringComparison.OrdinalIgnoreCase)))
            {
                bFromTemplate = true;
                var structure = plan.StructureSet.Structures.FirstOrDefault(x => x.Id.Equals(templateId, StringComparison.OrdinalIgnoreCase));

                if (structure != null && !structure.IsEmpty)
                {

                    return structure;
                }
                else if (structure.IsEmpty && autoGenerate && writeable && canBuildStructure)//generate structure if empty.
                {
                    var new_structure = StructureGenerationService.BuildStructureWithESAPI(_app, structure.Id, comment, true, plan, StructureDictionaryService);
                    return new_structure;
                }
                else//return empty structure. 
                {
                    //what do do with an empty structure.
                    return structure;
                }

            }
            // SECOND: If exact match is not there, check to see if it is part of the Structure Dictionary
            //check templateId against structure id. 
            StructureDictionaryModel structureDictionary = StructureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.ToLower().Equals(templateId.ToLower()));

            // This means there was no direct match to a key in the structure 
            /*if (structureDictionary == null)
            {
                // This checks to see if it is a structure
                //check structure Id agains the template matches. 
                string structureID = StructureDictionaryService.FindMatch(id);

                if (!String.IsNullOrEmpty(structureID))
                    structureDictionary = StructureDictionaryService.StructureDictionary.FirstOrDefault(s => s.StructureID.ToLower() == structureID.ToLower());
            }*/

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

                if (structure != null && !structure.IsEmpty)
                {
                    return structure;
                }
                else if (structure != null && structure.IsEmpty && autoGenerate && writeable && canBuildStructure)//generate structure if empty.
                {
                    var new_structure = StructureGenerationService.BuildStructureWithESAPI(_app, structure.Id, comment, true, plan, StructureDictionaryService);
                    return new_structure;
                }
            }

            // See if you can find it based on just stucture Code
            if (code != null && code.ToLower() != "control" && code.ToLower() != "ptv" && code.ToLower() != "ctv" && code.ToLower() != "gtv")//do not try to match control structures, they will be mismatched
            {
                if (plan.StructureSet.Structures.Where(x => x.StructureCodeInfos.Any()).Any(y => y.StructureCodeInfos.FirstOrDefault().Code == code) && !autoGenerate)
                {
                    return plan.StructureSet.Structures.Where(x => x.StructureCodeInfos.Any()).FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == code);
                }
            }

            // If no match, create it. 
            if (autoGenerate && writeable && !String.IsNullOrEmpty(comment) && canBuildStructure)
            {
                var structure = StructureGenerationService.BuildStructureWithESAPI(_app, id, comment, false, plan, StructureDictionaryService);
                return structure;
            }

            //if (plan.StructureSet.Structures.Where(x => x.StructureCodeInfos.Any()).Any(y => y.StructureCodeInfos.FirstOrDefault().Code == code) && !autoGenerate)
            //{
            //    return plan.StructureSet.Structures.Where(x => x.StructureCodeInfos.Any()).FirstOrDefault(x => x.StructureCodeInfos.FirstOrDefault().Code == code);
            //}
            //else
            //{//if structure doesn't exist, create it. 
            //    if (autoGenerate && writeable)
            //    {
            //        var structure = StructureGenerationService.BuildStructureWithESAPI(_app, id, comment, false, plan);
            //        return structure;
            //    }
            //    return null;
            //}

            // No match at all.
            return null;

        }

    }
}
