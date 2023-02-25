using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using PlanScoreCard.Models;
using PlanScoreCard.Models.Internals;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Printing;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DVHViewer2.Models
{
    public class StructurePlotItem: BindableBase
    {
        private DVHPlotModel plotmodel;

        // Class to contain:
        // Structure (Id, color, etc)
        // List of metrics with:
        // metric names, score bars, etc
        private StructureModel sm { get;set; }     
        public string id { get; set; }

        public SolidColorBrush Color { get; set; }

        private bool isChecked;

        public bool IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }

        public DelegateCommand CheckedCommand { get; set; }

        private OxyPlot.Series.LineSeries dvhSeries;

        public ObservableCollection<MetricPlotItem> MetricPlotItems { get; set; }
        public StructurePlotItem(StructureModel sm, DVHPlotModel plotmodel, OxyColor color)
        {
            this.plotmodel = plotmodel;
            this.Color = new SolidColorBrush(color.ToColor());
            this.sm = sm;
            this.id = sm.StructureId;
          
            //this.IDBlock.Text = "hi";
            //this.IDBlock.C

            this.MetricPlotItems = new ObservableCollection<MetricPlotItem>();
            this.CheckedCommand = new DelegateCommand(OnChecked);

            this.dvhSeries = plotmodel.GetDVHForId(this.id);
            //this.str = $"[{stm.OutputUnit}]{stm.MetricType}({stm.InputValue}{stm.InputUnit})";
        }

        private void OnChecked()
        {
            
            if (IsChecked && !plotmodel.Series.Contains(dvhSeries))
            {
                plotmodel.Series.Add(dvhSeries);
            }
            else if (!IsChecked && plotmodel.Series.Contains(dvhSeries))
            {
                plotmodel.Series.Remove(dvhSeries);
            }
            plotmodel.InvalidatePlot(true);

            foreach (var mpi in MetricPlotItems)
            {
                mpi.IsChecked = this.IsChecked;
                mpi.CheckedCommand.Execute();   
            }
        }

        public void AddMetricPlotItem(MetricPlotItem mpi)
        {
            this.MetricPlotItems.Add(mpi);
        }
    }
}
