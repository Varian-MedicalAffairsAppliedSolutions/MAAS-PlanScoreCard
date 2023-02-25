using OxyPlot;
using PlanScoreCard.Models.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Annotations;
using System.Windows;
using VMS.TPS.Common.Model.API;
using System.Runtime.InteropServices;
using VMS.TPS.Common.Model.Types;

namespace DVHViewer2.Models
{
    public class ScoreBarModel
    {
        private double RxGy { get; set; }
        private float CapWidthX { get; set; }
        private float CapWidthY { get; set; }
        public LineAnnotation Cap1 { get; set; }
        public LineAnnotation Cap2 { get; set;}
        public PolylineAnnotation ScoreBar { get; set; }
        public ScoreBarModel(ScoreTemplateModel st, double RxGy, Structure structure, String unit)
        {
            // Add 3 line annotations
            
            ScoreBar = new PolylineAnnotation();

            ScoreBar.LineStyle = LineStyle.Solid;
            ScoreBar.StrokeThickness = 1;
            ScoreBar.Color = OxyColor.FromRgb(structure.Color.R, structure.Color.G, structure.Color.B);
            ScoreBar.LineStyle = LineStyle.LongDash;

            CapWidthX = unit.Equals("cGy")?40F:0.4F;
            CapWidthY = 1.5F;
            this.RxGy = RxGy;
            
            var sorted_score_pts = st.ScorePoints.OrderBy(o => o.Score).ToList();


            if (st.MetricType == "VolumeAtDose")
            {
                // Ensure dose in Gy
                //dose unit needs to match the unit of the DVH. 
                double dose_val = GetDoseValueAtPlanUnit(st.InputUnit, unit, st.InputValue);
                // VaD --> vertical line
                //you should interpolate these lines. 
                for (int i = 0; i < sorted_score_pts.Count; i++)
                {
                    var pt = sorted_score_pts[i];
                    var vol_val = st.OutputUnit.ToString() == "%" ? pt.PointX : (pt.PointX / structure.Volume) * 100;

                    if (i == 0)
                    {
                        Cap1 = new LineAnnotation();
                        Cap1.LineStyle = LineStyle.Solid;
                        Cap1.Color = OxyColors.Red;
                        Cap1.StrokeThickness = 3;
                        Cap1.MinimumX = dose_val - (CapWidthX / 2);
                        Cap1.MaximumX = dose_val + (CapWidthX / 2);
                        Cap1.Type = LineAnnotationType.Horizontal;
                        Cap1.Y = vol_val;


                    }
                    else if (i == sorted_score_pts.Count - 1)
                    {

                        Cap2 = new LineAnnotation();
                        Cap2.LineStyle = LineStyle.Solid;
                        Cap2.Color = OxyColors.Green;
                        Cap2.StrokeThickness = 3;
                        Cap2.MinimumX = dose_val - (CapWidthX / 2);
                        Cap2.MaximumX = dose_val + (CapWidthX / 2);
                        Cap2.Type = LineAnnotationType.Horizontal;
                        Cap2.Y = vol_val;
                    }

                    ScoreBar.Points.Add(new DataPoint(dose_val, vol_val));

                }


            }

            else if (st.MetricType == "DoseAtVolume")
            {
                // Ensure vol in pct
                var vol_val = st.InputUnit.ToString() == "%" ? st.InputValue : (st.InputValue / structure.Volume) * 100;

                for(int i = 0; i < sorted_score_pts.Count; i++)
                {
                    var pt = sorted_score_pts[i];
                    var dose_val = GetDoseValueAtPlanUnit(st.OutputUnit, unit, pt.PointX);//st.OutputUnit.ToString() == "%" ? this.RxGy * (pt.PointX / 100) : pt.PointX;

                    if (i == 0)
                    {
                        Cap1 = new LineAnnotation();
                        Cap1.LineStyle = LineStyle.Solid;
                        Cap1.Color = OxyColors.Red;
                        Cap1.StrokeThickness = 3;
                        Cap1.MinimumY = vol_val - (CapWidthY / 2);
                        Cap1.MaximumY = vol_val + (CapWidthY / 2);
                        Cap1.Type = LineAnnotationType.Vertical;
                        Cap1.X = dose_val;

                    }
                    else if (i == sorted_score_pts.Count - 1)
                    {

                        Cap2 = new LineAnnotation();
                        Cap2.LineStyle = LineStyle.Solid;
                        Cap2.Color = OxyColors.Green;
                        Cap2.StrokeThickness = 3;
                        Cap2.MinimumY = vol_val - (CapWidthY / 2);
                        Cap2.MaximumY = vol_val + (CapWidthY / 2);
                        Cap2.Type = LineAnnotationType.Vertical;
                        Cap2.X = dose_val;
                    }


                    ScoreBar.Points.Add(new DataPoint(dose_val, vol_val));
                }


            }

            else if (st.MetricType == "MeanDose")
            {
                //MessageBox.Show("Still need to implement mean dose");
                // Draw red diamond on x at lowest and green diamond at highest
                var pt0 = sorted_score_pts.First();
                var ptf = sorted_score_pts.Last();

                Cap1 = new LineAnnotation();
                Cap1.LineStyle = LineStyle.Solid;
                Cap1.Color = OxyColors.Red;
                Cap1.StrokeThickness = 6;
                Cap1.MinimumY = - (CapWidthY / 2);
                Cap1.MaximumY = (CapWidthY / 2);
                Cap1.Type = LineAnnotationType.Vertical;
                Cap1.X = pt0.PointX;

                Cap2 = new LineAnnotation();
                Cap2.LineStyle = LineStyle.Solid;
                Cap2.Color = OxyColors.Green;
                Cap2.StrokeThickness = 6;
                Cap2.MinimumY = - (CapWidthY / 2);
                Cap2.MaximumY = (CapWidthY / 2);
                Cap2.Type = LineAnnotationType.Vertical;
                Cap2.X = ptf.PointX;
       
                foreach (var pt in sorted_score_pts)
                {
                    var dose_val = GetDoseValueAtPlanUnit(st.OutputUnit, unit, pt.PointX);// st.OutputUnit.ToString() == "%" ? this.RxGy * (pt.PointX / 100) : pt.PointX;
                    ScoreBar.Points.Add(new DataPoint(dose_val, 0));
                }

            }
        }

        private double GetDoseValueAtPlanUnit(string stUnit, string unit, double toConvert)
        {
            var dose_val = toConvert;// st.InputUnit.ToString() == "%" ? this.RxGy * (st.InputValue / 100) : st.InputValue;
            if (stUnit != unit)
            {
                if (stUnit == "%")
                {
                    if (unit == "Gy")
                    {
                        dose_val = this.RxGy * toConvert / 100;
                    }
                    else
                    {
                        dose_val = this.RxGy * toConvert;
                    }
                }
                else
                {
                    if (stUnit == "Gy")
                    {
                        //this means plan is in cGy
                        dose_val = toConvert * 100.0;
                    }
                    else
                    {
                        dose_val = toConvert / 100.0;
                    }
                }
            }

            return dose_val;
        }
    }
}
